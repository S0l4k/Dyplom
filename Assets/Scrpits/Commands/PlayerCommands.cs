using Unity.VisualScripting;
using UnityEngine;

namespace Commands
{
   
    public partial class CommandsManager : MonoBehaviour
    {
        public GameObject konon;
        [Command("Konon", "Shows us a godly creature")]
        public void KononCommand()
        {
            konon.SetActive(true);
        }
    }
}
