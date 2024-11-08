using Gameplay.UI;

namespace Gameplay
{
    public class MainStage : GameStage
    {
        protected override void OnEnter()
        {
            GameManager.UISystem.Show<MainView>();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnExit()
        {
            GameManager.UISystem.HideByFlag(Flags.Main);
        }
    }
}