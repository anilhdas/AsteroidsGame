using UnityEngine;
using PathCreation;

namespace AsteroidGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endInstruction;
        public float speed;

        Rigidbody2D _rigidbody2D;
        float _distanceTravelled;

        void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            _distanceTravelled += speed * Time.fixedDeltaTime;
            var _targetPosition = pathCreator.path.GetPointAtDistance(_distanceTravelled, endInstruction);
            _rigidbody2D.MovePosition(_targetPosition);
        }
    }
}
