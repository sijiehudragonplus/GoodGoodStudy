namespace Gameplay.UI
{
    public class MainView : View
    {
        public override ViewFlag Flag => Flags.Main;

        protected override string GetPrefab() => "MainView";

        protected override void OnShow()
        {
        }

        protected override void OnHide()
        {
        }

        [Binding("LogoutButton")]
        private void OnLogoutButtonClick()
        {
            Account.Logout();
            GameManager.GotoStage(new LoginStage());
        }
    }
}