using UnityEngine;

namespace LccHotfix
{
    public class CameraController : AObjectBase
    {
        public GameObject gameObject => GetParent<GameObjectEntity>().gameObject;

        private Vector2 _first;
        private Vector2 _second;
        private Vector3 vector;
        private bool _isNeedMove;
        public override void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _first = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                _second = Input.mousePosition;
                Vector3 first = Camera.main.ScreenToWorldPoint(new Vector3(_first.x, _first.y, 0));
                Vector3 second = Camera.main.ScreenToWorldPoint(new Vector3(_second.x, _second.y, 0));
                vector = second - first;
                _first = _second;
                _isNeedMove = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                _isNeedMove = false;
            }
            if (_isNeedMove)
            {
                float x = gameObject.transform.position.x;
                float y = gameObject.transform.position.y;
                x -= vector.x;
                y -= vector.y;
                gameObject.transform.position = new Vector3(x, y, gameObject.transform.position.z);
            }
        }
    }
}