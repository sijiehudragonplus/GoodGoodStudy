using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public interface IUISystem : IGameSystem
{
    TView Show<TView>() where TView : View, new();

    void Hide(View view);

    void HideByFlag(ViewFlag flag);
}

public readonly struct ViewFlag : IEquatable<ViewFlag>
{
    private static int s_Counter = 0;

    public static ViewFlag NewFlag() => s_Counter < 31 ? new ViewFlag(1 << s_Counter++) : throw new IndexOutOfRangeException();

    private readonly int m_Value;

    private ViewFlag(int value)
    {
        m_Value = value;
    }

    public bool HasAny(ViewFlag other)
    {
        return (m_Value & other.m_Value) != 0;
    }

    public bool HasAll(ViewFlag other)
    {
        return (m_Value & other.m_Value) == other.m_Value;
    }

    public static ViewFlag operator &(in ViewFlag a, in ViewFlag b)
    {
        return new(a.m_Value & b.m_Value);
    }

    public static ViewFlag operator |(in ViewFlag a, in ViewFlag b)
    {
        return new(a.m_Value | b.m_Value);
    }

    public bool Equals(ViewFlag other)
    {
        return m_Value == other.m_Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ViewFlag other && Equals(other);
    }

    public override int GetHashCode()
    {
        return m_Value;
    }
}

public abstract class View
{
    public abstract ViewFlag Flag { get; }

    public GameObject    GameObject { get; private set; }
    public RectTransform Transform  { get; private set; }

    private bool       m_AutoShowWithParent;
    private List<View> Children = new List<View>();

    internal void Instantiate(Transform root)
    {
        GameObject = Object.Instantiate(GameManager.AssetSystem.Load<GameObject>(GetPrefab()), root);
        Transform  = (RectTransform) GameObject.transform;
        GameObject.SetActive(false);
    }

    internal void Destroy()
    {
        Object.Destroy(GameObject);
        GameObject = null;
        Transform  = null;
    }

    internal void Show()
    {
        if (GameObject.activeSelf) return;
        GameObject.SetActive(true);
        BindingAttribute.Bind(this);
        OnShow();
        foreach (var child in Children)
        {
            if (child.m_AutoShowWithParent)
                child.Show();
        }
    }

    internal void Hide()
    {
        if (!GameObject.activeSelf) return;
        GameObject.SetActive(false);
        foreach (var child in Children)
        {
            child.Hide();
        }

        OnHide();
    }

    protected abstract string GetPrefab();

    protected abstract void OnShow();

    protected abstract void OnHide();

    internal View AddChildView(Type viewType, GameObject gameObject, bool autoShow)
    {
        View view = (View) Activator.CreateInstance(viewType);
        view.GameObject           = gameObject;
        view.Transform            = (RectTransform) gameObject.transform;
        view.m_AutoShowWithParent = autoShow;
        Children.Add(view);

        if (autoShow)
        {
            view.Show();
        }
        else
        {
            gameObject.SetActive(false);
        }

        return view;
    }

    public TView AddChildView<TView>(GameObject gameObject, bool autoShowWithParent = true) where TView : View, new()
    {
        return (TView) AddChildView(typeof(TView), gameObject, autoShowWithParent);
    }

    public TView AddChildView<TView>(Transform parent, bool autoShowWithParent = true) where TView : View, new()
    {
        TView view = new TView();
        view.Instantiate(parent);
        view.m_AutoShowWithParent = autoShowWithParent;
        Children.Add(view);

        if (GameObject.activeSelf)
        {
            view.Show();
        }

        return view;
    }

    public void RemoveChildView(View view)
    {
        view.Hide();
        view.Destroy();
        Children.Remove(view);
    }

    public void ShowChildView(View view)
    {
        view.Show();
    }

    public void HideChildView(View view)
    {
        view.Hide();
    }
}

internal sealed class UISystem : IUISystem
{
    private GameObject m_UIRoot;
    private Canvas     m_Canvas;

    private List<View> m_Views = new List<View>();

    public void OnStart()
    {
        m_UIRoot      = Object.Instantiate(GameManager.AssetSystem.Load<GameObject>("UIRoot"));
        m_UIRoot.name = "UIRoot";
        Object.DontDestroyOnLoad(m_UIRoot);

        m_Canvas = m_UIRoot.GetComponentInChildren<Canvas>();
    }

    public void OnUpdate()
    {
    }

    public void OnDestroy()
    {
    }

    public TView Show<TView>() where TView : View, new()
    {
        TView view = new TView();
        m_Views.Add(view);
        view.Instantiate(m_Canvas.transform);
        view.Show();
        return view;
    }

    public void Hide(View view)
    {
        if (m_Views.Remove(view))
        {
            view.Hide();
            view.Destroy();
        }
        else
        {
            Debug.LogError($"{view}已经被hide了 无法再次hide");
        }
    }

    public void HideByFlag(ViewFlag flag)
    {
        for (var i = m_Views.Count - 1; i >= 0; i--)
        {
            if (m_Views[i].Flag.HasAll(flag))
            {
                Hide(m_Views[i]);
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, Inherited = false)]
public sealed class BindingAttribute : PreserveAttribute
{
    /// <summary>
    /// 绑定UI根节点到字段或事件函数，绑定函数时，若未指定componentType和componentEventName，则默认为Button.onClick
    /// </summary>
    public BindingAttribute() : this(string.Empty)
    {
    }

    /// <summary>
    /// 绑定字段或事件函数，绑定函数时，若未指定componentType和componentEventName，则默认为Button.onClick
    /// </summary>
    /// <param name="path">GameObject相对UI根节点路径</param>
    public BindingAttribute(string path) : this(path, typeof(Button), nameof(Button.onClick))
    {
    }

    /// <summary>
    /// 绑定事件函数
    /// </summary>
    /// <param name="componentType">绑定事件组件类型，仅绑定函数时有效</param>
    /// <param name="componentEventName">绑定事件成员名称，仅绑定函数时有效</param>
    public BindingAttribute(Type componentType, string componentEventName)
        : this(string.Empty, componentType, componentEventName)
    {
    }

    /// <summary>
    /// 绑定事件函数
    /// </summary>
    /// <param name="path">GameObject相对UI根节点路径</param>
    /// <param name="componentType">绑定事件组件类型，仅绑定函数时有效</param>
    /// <param name="componentEventName">绑定事件成员名称，仅绑定函数时有效</param>
    public BindingAttribute(string path, Type componentType, string componentEventName)
    {
        Path               = path;
        ComponentType      = componentType;
        ComponentEventName = componentEventName;
    }

    public readonly string Path;
    public readonly Type   ComponentType;
    public readonly string ComponentEventName;

    /// <summary>
    /// 当是SubView时，是否自动显示
    /// </summary>
    public bool AutoShow = true;

    /// <summary>
    /// 能否允许为空，若允许，则找不到时不会有任何报错提示
    /// </summary>
    public bool CanBeNull = false;

    private static readonly object[] Params = new object[1];

    public static void Bind(View view)
    {
        var type = view.GetType();
        while (type != null)
        {
            try
            {
                BindFields(type, view);
                BindMethods(type, view);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            type = type.BaseType;
        }

        Params[0] = null;
    }

    private static readonly Dictionary<Type, FieldInfo[]> FieldInfos = new();

    private static FieldInfo[] GetFields(Type type)
    {
        if (FieldInfos.TryGetValue(type, out var result))
        {
            return result;
        }

        result = type.GetFields(BindingFlags.Instance  |
                                BindingFlags.Public    |
                                BindingFlags.NonPublic |
                                BindingFlags.DeclaredOnly);
        FieldInfos[type] = result;
        return result;
    }

    private static readonly Dictionary<FieldInfo, BindingAttribute> FieldBindingAttributes      = new();
    private static readonly HashSet<FieldInfo>                      FieldEmptyBindingAttributes = new();

    private static BindingAttribute GetFieldBindingAttribute(FieldInfo fieldInfo)
    {
        if (FieldEmptyBindingAttributes.Contains(fieldInfo))
        {
            return null;
        }

        if (FieldBindingAttributes.TryGetValue(fieldInfo, out var bindingAttribute))
        {
            return bindingAttribute;
        }

        bindingAttribute = fieldInfo.GetCustomAttribute<BindingAttribute>(false);
        if (bindingAttribute == null)
        {
            FieldEmptyBindingAttributes.Add(fieldInfo);
            return null;
        }

        FieldBindingAttributes[fieldInfo] = bindingAttribute;

        return bindingAttribute;
    }

    private static void BindFields(Type type, View view)
    {
        foreach (var fieldInfo in GetFields(type))
        {
            var binding = GetFieldBindingAttribute(fieldInfo);

            if (binding == null) continue;

            var bindingTarget = string.IsNullOrEmpty(binding.Path)
                ? view.Transform
                : view.Transform.Find(binding.Path);
            if (bindingTarget == null)
            {
                if (binding.CanBeNull == false)
                {
                    var declaringType = fieldInfo.DeclaringType ?? type;
                    Debug.LogError(
                        $"{view.Transform.name}下{declaringType.Name}.{fieldInfo.Name}绑定对象{binding.Path}不存在!");
                }

                continue;
            }

            if (fieldInfo.FieldType == typeof(GameObject))
            {
                fieldInfo.SetValue(view, bindingTarget.gameObject);
            }
            else if (fieldInfo.FieldType == typeof(Transform))
            {
                fieldInfo.SetValue(view, bindingTarget);
            }
            else if (fieldInfo.FieldType.IsSubclassOf(typeof(View)))
            {
                fieldInfo.SetValue(view,
                    view.AddChildView(fieldInfo.FieldType, bindingTarget.gameObject, binding.AutoShow));
            }
            else if (bindingTarget.gameObject.TryGetComponent(fieldInfo.FieldType, out var component))
            {
                fieldInfo.SetValue(view, component);
            }
            else if (binding.CanBeNull == false)
            {
                var declaringType = fieldInfo.DeclaringType ?? type;
                Debug.LogError(
                    $"{view.Transform.name}下{declaringType.Name}.{fieldInfo.Name}绑定对象{binding.Path}上不存在脚本{fieldInfo.FieldType}!");
            }
        }
    }

    private static readonly Dictionary<Type, MethodInfo[]> MethodInfos = new();

    private static MethodInfo[] GetMethods(Type type)
    {
        if (MethodInfos.TryGetValue(type, out var result))
        {
            return result;
        }

        result = type.GetMethods(BindingFlags.Instance  |
                                 BindingFlags.Public    |
                                 BindingFlags.NonPublic |
                                 BindingFlags.DeclaredOnly);

        MethodInfos[type] = result;
        return result;
    }

    private static readonly Dictionary<MethodInfo, BindingAttribute> MethodBindingAttributes      = new();
    private static readonly HashSet<MethodInfo>                      MethodEmptyBindingAttributes = new();

    private static BindingAttribute GetMethodBindingAttribute(MethodInfo methodInfo)
    {
        if (MethodEmptyBindingAttributes.Contains(methodInfo))
        {
            return null;
        }

        if (MethodBindingAttributes.TryGetValue(methodInfo, out var bindingAttribute))
        {
            return bindingAttribute;
        }

        bindingAttribute = methodInfo.GetCustomAttribute<BindingAttribute>(false);

        if (bindingAttribute == null)
        {
            MethodEmptyBindingAttributes.Add(methodInfo);

            return null;
        }

        MethodBindingAttributes[methodInfo] = bindingAttribute;
        return bindingAttribute;
    }

    private static void BindMethods(Type type, View view)
    {
        foreach (var methodInfo in GetMethods(type))
        {
            var binding = GetMethodBindingAttribute(methodInfo);

            if (binding == null) continue;

            var bindingTarget = string.IsNullOrEmpty(binding.Path)
                ? view.Transform
                : view.Transform.Find(binding.Path);
            if (bindingTarget == null)
            {
                if (binding.CanBeNull == false)
                {
                    var declaringType = methodInfo.DeclaringType ?? type;
                    Debug.LogError($"{declaringType.Name}.{methodInfo.Name}绑定对象{binding.Path}不存在!");
                }

                continue;
            }

            if (bindingTarget.gameObject.TryGetComponent(binding.ComponentType, out var component) == false)
            {
                if (binding.CanBeNull == false)
                {
                    var declaringType = methodInfo.DeclaringType ?? type;
                    Debug.LogError(
                        $"{declaringType.Name}.{methodInfo.Name}绑定对象{binding.Path}上不存在脚本{binding.ComponentType}!");
                }

                continue;
            }

            var eventBase = GetEventObject(binding.ComponentType, component, binding.ComponentEventName);
            if (eventBase == null)
            {
                if (binding.CanBeNull == false)
                {
                    var declaringType = methodInfo.DeclaringType ?? type;
                    Debug.LogError(
                        $"{declaringType.Name}.{methodInfo.Name}绑定对象{binding.Path}({binding.ComponentType.Name})不存在事件{binding.ComponentEventName}!");
                }

                continue;
            }

            var addListener   = GetAddListenerMethod(eventBase.GetType());
            var eventCallback = methodInfo.CreateDelegate(addListener.GetParameters()[0].ParameterType, view);

            Params[0] = eventCallback;
            addListener.Invoke(eventBase, Params);
            Params[0] = null;
        }
    }

    private static readonly Dictionary<Type, MethodInfo> AddListenerMethods = new();

    private static MethodInfo GetAddListenerMethod(Type type)
    {
        if (AddListenerMethods.TryGetValue(type, out var result))
        {
            return result;
        }

        result = type.GetMethod("AddListener", BindingFlags.Instance | BindingFlags.Public);
        if (result == null)
        {
            throw new NullReferenceException();
        }

        AddListenerMethods[type] = result;
        return result;
    }

    private static object GetEventObject(Type componentType, object componentObject, string eventName)
    {
        var eventProperty = GetUnityEventPropertyInfo(componentType, eventName);
        if (eventProperty != null)
        {
            return eventProperty.GetValue(componentObject);
        }

        var eventField = GetUnityEventFieldInfo(componentType, eventName);
        if (eventField != null)
        {
            return eventField.GetValue(componentObject);
        }

        return null;
    }

    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> UnityEventPropertyInfos      = new();
    private static readonly Dictionary<Type, HashSet<string>>                  UnityEventEmptyPropertyInfos = new();

    private static PropertyInfo GetUnityEventPropertyInfo(Type type, string propertyName)
    {
        if (UnityEventEmptyPropertyInfos.TryGetValue(type, out var emptyNames))
        {
            if (emptyNames.Contains(propertyName))
            {
                return null;
            }
        }

        if (UnityEventPropertyInfos.TryGetValue(type, out var result))
        {
            if (result.TryGetValue(propertyName, out var property))
            {
                return property;
            }
        }

        var eventProperty = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (eventProperty == null || !eventProperty.PropertyType.IsSubclassOf(typeof(UnityEventBase)))
        {
            if (emptyNames != null)
            {
                emptyNames.Add(propertyName);
            }
            else
            {
                emptyNames                         = new HashSet<string>() {propertyName};
                UnityEventEmptyPropertyInfos[type] = emptyNames;
            }

            return null;
        }

        if (result != null)
        {
            result[propertyName] = eventProperty;
        }
        else
        {
            result = new Dictionary<string, PropertyInfo>
            {
                [propertyName] = eventProperty
            };
            UnityEventPropertyInfos[type] = result;
        }

        return eventProperty;
    }

    private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> UnityEventFieldInfos      = new();
    private static readonly Dictionary<Type, HashSet<string>>               UnityEventEmptyFieldInfos = new();

    private static FieldInfo GetUnityEventFieldInfo(Type type, string fieldName)
    {
        if (UnityEventEmptyFieldInfos.TryGetValue(type, out var emptyNames))
        {
            if (emptyNames.Contains(fieldName))
            {
                return null;
            }
        }

        if (UnityEventFieldInfos.TryGetValue(type, out var result))
        {
            if (result.TryGetValue(fieldName, out var property))
            {
                return property;
            }
        }

        var eventField = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        if (eventField == null || !eventField.FieldType.IsSubclassOf(typeof(UnityEventBase)))
        {
            if (emptyNames != null)
            {
                emptyNames.Add(fieldName);
            }
            else
            {
                emptyNames                      = new HashSet<string>() {fieldName};
                UnityEventEmptyFieldInfos[type] = emptyNames;
            }

            return null;
        }

        if (result != null)
        {
            result[fieldName] = eventField;
        }
        else
        {
            result = new Dictionary<string, FieldInfo>
            {
                [fieldName] = eventField
            };
            UnityEventFieldInfos[type] = result;
        }

        return eventField;
    }
}