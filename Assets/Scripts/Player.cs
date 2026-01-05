using Mirror;
using UnityEngine;
using TMPro;

namespace QuickStart
{
    public class PlayerScript : NetworkBehaviour
    {
        public TextMeshPro playerNameText;
        public GameObject floatingInfo;

        private Material _playerMaterialClone;
        private SceneScript _sceneScript;

        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color playerColor = Color.white;

        void OnNameChanged(string _Old, string _New)
        {
            playerNameText.text = _New;
        }

        void OnColorChanged(Color _Old, Color _New)
        {
            playerNameText.color = _New;
            _playerMaterialClone = new Material(GetComponent<Renderer>().material)
            {
                color = _New
            };

            GetComponent<Renderer>().material = _playerMaterialClone;
        }

        public override void OnStartLocalPlayer()
        {
            _sceneScript.playerScript = this;

            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, 0);

            floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
            floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            string name = "Player" + Random.Range(100, 999);
            Color color = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            CmdSetupPlayer(name, color);
        }

        [Command]
        public void CmdSetupPlayer(string _name, Color _col)
        {
            playerName = _name;
            playerColor = _col;

            _sceneScript.statusText = $"{playerName} joined.";
        }

        [Command]
        public void CmdSendPlayerMessage()
        {
            if (_sceneScript)
                _sceneScript.statusText = $"{playerName} says hello {Random.Range(10, 99)}";
        }

        private void Awake()
        {
            _sceneScript = FindFirstObjectByType<SceneScript>();
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                floatingInfo.transform.LookAt(Camera.main.transform);
                return;
            }

            float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
            float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

            transform.Rotate(0, moveX, 0);
            transform.Translate(0, 0, moveZ);
        }
    }
}