using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.RazorEngine.TestTypes.BaseTypes
{
    /// <summary>
    /// Test Type.
    /// </summary>
    public class TemplateViewData
    {
        /// <summary>
        /// Test Type.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Test Type.
        /// </summary>
        public object Model { get; set; }
    }
    /// <summary>
    /// Test Type.
    /// </summary>
    public class DataAccessor { }
    /// <summary>
    /// Test Type.
    /// </summary>
    public class ObjectFactory
    {
        /// <summary>
        /// Test Type.
        /// </summary>
        public static T GetInstance<T>() where T : new()
        {
            return new T();
        }
    }
    /// <summary>
    /// Test Type.
    /// </summary>
    public interface ILocalizer
    {
        /// <summary>
        /// Test Type.
        /// </summary>
        string Language { get; }
    }
    /// <summary>
    /// Test Type.
    /// </summary>
    public class DatabindedLocalizer : ILocalizer
    {
        string lang;
        /// <summary>
        /// Test Type.
        /// </summary>
        public DatabindedLocalizer(DataAccessor accessor, string language) { lang = language; }
        /// <summary>
        /// Test Type.
        /// </summary>
        public string Language { get { return lang; } }
    }
    // Depends on internal API
    /// <summary>
    /// Test Type.
    /// </summary>
    public class AddLanguageInfo_OverrideModelType<T> : TemplateBase<TemplateViewData>
    {
        private readonly DataAccessor _dataAccessor;

        /// <summary>
        /// Test Type.
        /// </summary>
        public AddLanguageInfo_OverrideModelType()
        {
            _dataAccessor = ObjectFactory.GetInstance<DataAccessor>();
        }

        /// <summary>
        /// Test Type.
        /// </summary>
        public new T Model { get { return (T)base.Model.Model; } }
        internal override Type ModeType
        {
            get
            {
                return typeof(T);
            }
        }

        /// <summary>
        /// Test Type.
        /// </summary>
        public ILocalizer Localizer { get; set; }

        /// <summary>
        /// Test Type.
        /// </summary>
        public override void SetModel(object model)
        {
            // Set the main model
            base.SetModel(model);

            // We need a localizer, so we can localize the text, so we can write @Localizer.GetText() instead of @Model.Localizer.GetText()
            Localizer = new DatabindedLocalizer(_dataAccessor, base.Model.Language);
        }
    }

    /// <summary>
    /// Test Type.
    /// </summary>
    public class AddLanguageInfo_OverrideInclude<T> : TemplateBase<T>
    {
        private readonly DataAccessor _dataAccessor;

        private TemplateViewData _templateViewData;
        /// <summary>
        /// Test Type.
        /// </summary>
        public AddLanguageInfo_OverrideInclude()
        {
            _dataAccessor = ObjectFactory.GetInstance<DataAccessor>();
        }

        /// <summary>
        /// Test Type.
        /// </summary>
        public ILocalizer Localizer { get; set; }

        /// <summary>
        /// Test Type.
        /// </summary>
        public override void SetModel(object model)
        {
            // The viewmodel passed to the template
            _templateViewData = (TemplateViewData)model;

            // We need a localizer, so we can localize the text, so we can write @Localizer.GetText() instead of @Model.Localizer.GetText()
            Localizer = new DatabindedLocalizer(_dataAccessor, _templateViewData.Language);

            // Set the main model
            base.SetModel(_templateViewData.Model);
        }

        /// <summary>
        /// Test Type.
        /// </summary>
        public override TemplateWriter Include(string cacheName, object model = null, Type modelType = null)
        {
            // When model == null we use our current model => we should use the same modelType as well.
            return base.Include(cacheName, model ?? _templateViewData, model == null ? typeof(T) : modelType);
        }

        /// <summary>
        /// Test Type.
        /// </summary>
        protected override ITemplate ResolveLayout(string name)
        {
            // We need to resolve the ITemplate with the TemplateViewData instead of the model, otherwise, the SetModel on the Layout will fail
            return InternalTemplateService.Resolve(name, _templateViewData, typeof(T), (DynamicViewBag)ViewBag, ResolveType.Layout);
        }
    }


    /// <summary>
    /// Test Type.
    /// </summary>
    public class AddLanguageInfo_Viewbag<T> : TemplateBase<T>
    {
        private readonly DataAccessor _dataAccessor;
        private ILocalizer _localizer;

        /// <summary>
        /// Test Type.
        /// </summary>
        public AddLanguageInfo_Viewbag()
        {
            _dataAccessor = ObjectFactory.GetInstance<DataAccessor>();
        }

        /// <summary>
        /// Test Type.
        /// </summary>
        public ILocalizer Localizer
        {
            get
            {
                if (_localizer == null)
                {
                    _localizer = new DatabindedLocalizer(_dataAccessor, ViewBag.Language);
                }
                return _localizer;
            }
        }
    }

    /// <summary>
    /// Test Type.
    /// </summary>
    public class AddLanguageInfo_Viewbag_SetModel<T> : TemplateBase<T>
    {
        private readonly DataAccessor _dataAccessor;
        /// <summary>
        /// Test Type.
        /// </summary>
        public AddLanguageInfo_Viewbag_SetModel()
        {
            _dataAccessor = ObjectFactory.GetInstance<DataAccessor>();
        }

        /// <summary>
        /// Test Type.
        /// </summary>
        public ILocalizer Localizer { get; set; }
        /// <summary>
        /// Test Type.
        /// </summary>
        public override void SetModel(object model)
        {            
            // We need a localizer, so we can localize the text, so we can write @Localizer.GetText() instead of @Model.Localizer.GetText()
            Localizer = new DatabindedLocalizer(_dataAccessor, ViewBag.Language);

            // Set the main model
            base.SetModel(model);
        }
    }
}
