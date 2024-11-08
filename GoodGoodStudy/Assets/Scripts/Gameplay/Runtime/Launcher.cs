using System;
using UnityEngine;

namespace Gameplay
{
    public class Launcher : MonoBehaviour
    {
        private void Start()
        {
            GameManager.GotoStage(new LoginStage());
        }
    }
}