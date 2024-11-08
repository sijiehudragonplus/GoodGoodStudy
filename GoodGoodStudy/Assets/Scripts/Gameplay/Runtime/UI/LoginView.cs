using System;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class LoginView : View
    {
        [Binding("AccountInput")]
        private TMP_InputField m_AccountInput;

        public override ViewFlag Flag => Flags.Login;

        protected override string GetPrefab() => "LoginView";

        protected override void OnShow()
        {
            m_AccountInput.text = PlayerPrefs.GetString("ACCOUNT", String.Empty);
        }

        protected override void OnHide()
        {
        }

        [Binding("LoginButton")]
        private void OnLoginButtonClick()
        {
            if (string.IsNullOrEmpty(m_AccountInput.text))
            {
                Debug.LogError("必须输入账号！");
                return;
            }

            PlayerPrefs.SetString("ACCOUNT", m_AccountInput.text);
            Account.Login(m_AccountInput.text);
            GameManager.GotoStage(new MainStage());
        }
    }
}