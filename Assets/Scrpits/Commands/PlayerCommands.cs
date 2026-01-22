using Unity.VisualScripting;
using UnityEngine;

namespace Commands
{
   
    public partial class CommandsManager : MonoBehaviour
    {
        public PlayerController player;
        public GameObject konon;
        [Command("Konon", "Shows us a godly creature")]
        public void KononCommand()
        {
            konon.SetActive(true);
        }
        [Command("speed", "Sets player walk speed")]
        public void SetSpeed(float value)
        {
            player.walkSpeed = value;
            Debug.Log("New speed: " + value);
        }
        [Command("sprint", "Sets sprint speed")]
        public void SetSprint(float value)
        {
            player.sprintSpeed = value;
        }
        [Command("god", "Infinite stamina + no jumpscare")]
        public void GodMode()
        {
            player.godMode = !player.godMode;

            if (player.godMode)
            {
                player.maxStamina = 9999;
                Debug.Log("GOD MODE ON");
            }
            else
            {
                player.maxStamina = 5f;
                Debug.Log("GOD MODE OFF");
            }
        }
        [Command("help", "Shows all commands with description")]
        public void Help()
        {
            Debug.Log("=== COMMANDS ===");
            foreach (var cmd in _commands.Values)
            {
                Debug.Log($"{cmd.attr.CommandName} - {cmd.attr.CommandDescription}");
            }
        }

    }
}
