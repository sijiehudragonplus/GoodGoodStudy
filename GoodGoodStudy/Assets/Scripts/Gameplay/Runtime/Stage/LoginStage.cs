using Gameplay.UI;

namespace Gameplay
{
    public class LoginStage : GameStage
    {
        protected override void OnEnter()
        {
            GameManager.UISystem.Show<LoginView>();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnExit()
        {
            GameManager.UISystem.HideByFlag(Flags.Login);
        }
    }
}