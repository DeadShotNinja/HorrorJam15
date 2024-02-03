using UnityEngine;
using HJ.Tools;

namespace HJ.Runtime
{
    public class ExaminePutter : MonoBehaviour
    {
        private PutSettings _putSettings;
        private Vector3 _putStartPos;
        private Quaternion _putStartRot;
        private bool _putStarted;

        private float _putPosT;
        private float _putPosVelocity;

        private float _putRotT;
        private float _putRotVelocity;

        public void Put(PutSettings putSettings)
        {
            _putSettings = putSettings;
            _putStartPos = putSettings.IsLocalSpace ? transform.localPosition : transform.position;
            _putStartRot = putSettings.IsLocalSpace ? transform.localRotation : transform.rotation;
            _putStarted = true;
        }

        private void Update()
        {
            if (!_putStarted) return;

            float putPosCurve = _putSettings.PutPositionCurve.Eval(_putPosT);
            _putPosT = Mathf.SmoothDamp(_putPosT, 1f, ref _putPosVelocity, _putSettings.PutPositionCurve.CurveTime + putPosCurve);

            if(!_putSettings.IsLocalSpace) transform.position = VectorExtension.QuadraticBezier(_putStartPos, _putSettings.PutPosition, _putSettings.PutControl, _putPosT);
            else transform.localPosition = VectorExtension.QuadraticBezier(_putStartPos, _putSettings.PutPosition, _putSettings.PutControl, _putPosT);

            float putRotCurve = _putSettings.PutRotationCurve.Eval(_putRotT);
            _putRotT = Mathf.SmoothDamp(_putRotT, 1f, ref _putRotVelocity, _putSettings.PutRotationCurve.CurveTime + putRotCurve);

            if (!_putSettings.IsLocalSpace) transform.rotation = Quaternion.Slerp(_putStartRot, _putSettings.PutRotation, _putRotT);
            else transform.localRotation = Quaternion.Slerp(_putStartRot, _putSettings.PutRotation, _putRotT);

            if ((_putPosT * _putRotT) >= 0.99f)
            {
                if (!_putSettings.IsLocalSpace)
                {
                    transform.SetPositionAndRotation(_putSettings.PutPosition, _putSettings.PutRotation);
                }
                else
                {
                    transform.localPosition = _putSettings.PutPosition;
                    transform.localRotation = _putSettings.PutRotation;
                }

                Destroy(this);
            }
        }
    }
}