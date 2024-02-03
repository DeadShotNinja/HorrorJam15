using UnityEngine;

namespace HJ.Runtime
{
    public class CustomInteractTitle : MonoBehaviour, IInteractTitle
    {
        [SerializeField] private ReflectionField _dynamicTitle;
        [SerializeField] private ReflectionField _dynamicUseTitle;
        [SerializeField] private ReflectionField _dynamicExamineTitle;

        [SerializeField] private bool _overrideTitle;
        [SerializeField] private bool _overrideUseTitle;
        [SerializeField] private bool _overrideExamineTitle;

        [SerializeField] private bool _useTitleDynamic;
        [SerializeField] private bool _useUseTitleDynamic;
        [SerializeField] private bool _useExamineTitleDynamic;

        [SerializeField] private GString _title;
        [SerializeField] private GString _trueTitle;
        [SerializeField] private GString _falseTitle;

        [SerializeField] private GString _useTitle;
        [SerializeField] private GString _trueUseTitle;
        [SerializeField] private GString _falseUseTitle;

        [SerializeField] private GString _examineTitle;
        [SerializeField] private GString _trueExamineTitle;
        [SerializeField] private GString _falseExamineTitle;

        public GString Title => _title;
        public GString UseTitle => _useTitle;
        public GString ExamineTitle => _examineTitle;
        
        private void Start()
        {
            if (_overrideTitle)
            {
                if (_useTitleDynamic)
                {
                    _trueTitle.SubscribeGloc();
                    _falseTitle.SubscribeGloc();
                    _title = _dynamicTitle.Value ? _trueTitle : _falseTitle;
                }
                else
                {
                    _title.SubscribeGloc();
                }
            }

            if (_overrideUseTitle)
            {
                if (_useUseTitleDynamic)
                {
                    _trueUseTitle.SubscribeGlocMany();
                    _falseUseTitle.SubscribeGlocMany();
                    _useTitle = _dynamicUseTitle.Value ? _trueUseTitle : _falseUseTitle;
                }
                else
                {
                    _useTitle.SubscribeGlocMany();
                }
            }

            if (_overrideExamineTitle)
            {
                if (_useExamineTitleDynamic)
                {
                    _trueExamineTitle.SubscribeGlocMany();
                    _falseExamineTitle.SubscribeGlocMany();
                    _examineTitle = _dynamicExamineTitle.Value ? _trueExamineTitle : _falseExamineTitle;
                }
                else
                {
                    _examineTitle.SubscribeGlocMany();
                }
            }
        }

        public TitleParams InteractTitle()
        {
            string title = _title;
            string useTitle = _useTitle;
            string examineTitle = _examineTitle;

            if (!_overrideTitle) title = null;
            else if (_useTitleDynamic) title = _dynamicTitle.Value ? _trueTitle : _falseTitle;

            if (!_overrideUseTitle) useTitle = null;
            else if (_useUseTitleDynamic) useTitle = _dynamicUseTitle.Value ? _trueUseTitle : _falseUseTitle;

            if (!_overrideExamineTitle) examineTitle = null;
            else if (_useExamineTitleDynamic) examineTitle = _dynamicExamineTitle.Value ? _trueExamineTitle : _falseExamineTitle;

            return new TitleParams()
            {
                Title = title,
                Button1 = useTitle,
                Button2 = examineTitle
            };
        }
    }
}