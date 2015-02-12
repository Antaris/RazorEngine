using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.RazorEngine.TestTypes.BaseTypes
{
    public class TemplateViewData
    {
        public string Language { get; set; }

        public object Model { get; set; }
    }
    public class DataAccessor { }
    public class ObjectFactory
    {
        public static T GetInstance<T>() where T : new()
        {
            return new T();
        }
    }
    public interface ILocalizer
    {
        string Language { get; }
    }
    public class DatabindedLocalizer : ILocalizer
    {
        string lang;
        public DatabindedLocalizer(DataAccessor accessor, string language) { lang = language; }
        public string Language { get { return lang; } }
    }
    // Depends on internal API
    public class AddLanguageInfo_OverrideModelType<T> : TemplateBase<TemplateViewData>
    {
        private readonly DataAccessor _dataAccessor;

        public AddLanguageInfo_OverrideModelType()
        {
            _dataAccessor = ObjectFactory.GetInstance<DataAccessor>();
        }

        public new T Model { get { return (T)base.Model.Model; } }
        internal override Type ModeType
        {
            get
            {
                return typeof(T);
            }
        }

        public ILocalizer Localizer { get; set; }

        public override void SetModel(object model)
        {
            // Set the main model
            base.SetModel(model);

            // We need a localizer, so we can localize the text, so we can write @Localizer.GetText() instead of @Model.Localizer.GetText()
            Localizer = new DatabindedLocalizer(_dataAccessor, base.Model.Language);
        }
    }

    public class AddLanguageInfo_OverrideInclude<T> : TemplateBase<T>
    {
        private readonly DataAccessor _dataAccessor;

        private TemplateViewData _templateViewData;
        public AddLanguageInfo_OverrideInclude()
        {
            _dataAccessor = ObjectFactory.GetInstance<DataAccessor>();
        }

        public ILocalizer Localizer { get; set; }

        public override void SetModel(object model)
        {
            // The viewmodel passed to the template
            _templateViewData = (TemplateViewData)model;

            // We need a localizer, so we can localize the text, so we can write @Localizer.GetText() instead of @Model.Localizer.GetText()
            Localizer = new DatabindedLocalizer(_dataAccessor, _templateViewData.Language);

            // Set the main model
            base.SetModel(_templateViewData.Model);
        }

        public override TemplateWriter Include(string cacheName, object model = null, Type modelType = null)
        {
            // When model == null we use our current model => we should use the same modelType as well.
            return base.Include(cacheName, model ?? _templateViewData, model == null ? typeof(T) : modelType);
        }

        protected override ITemplate ResolveLayout(string name)
        {
            // We need to resolve the ITemplate with the TemplateViewData instead of the model, otherwise, the SetModel on the Layout will fail
            return InternalTemplateService.Resolve(name, _templateViewData, typeof(T), (DynamicViewBag)ViewBag, ResolveType.Layout);
        }
    }


    public class AddLanguageInfo_Viewbag<T> : TemplateBase<T>
    {
        private readonly DataAccessor _dataAccessor;
        private ILocalizer _localizer;

        private TemplateViewData _templateViewData;
        public AddLanguageInfo_Viewbag()
        {
            _dataAccessor = ObjectFactory.GetInstance<DataAccessor>();
        }

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

    public class AddLanguageInfo_Viewbag_SetModel<T> : TemplateBase<T>
    {
        private readonly DataAccessor _dataAccessor;
        public AddLanguageInfo_Viewbag_SetModel()
        {
            _dataAccessor = ObjectFactory.GetInstance<DataAccessor>();
        }

        public ILocalizer Localizer { get; set; }
        public override void SetModel(object model)
        {            
            // We need a localizer, so we can localize the text, so we can write @Localizer.GetText() instead of @Model.Localizer.GetText()
            Localizer = new DatabindedLocalizer(_dataAccessor, ViewBag.Language);

            // Set the main model
            base.SetModel(model);
        }
    }
}
