﻿using System;
using System.Configuration;
using System.ComponentModel;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Spa;
using DevExpress.ExpressApp.Spa.AspNetCore;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;

namespace HRM.Spa {
    public partial class HRMSpaApplication : SpaApplication {
        #region Default XAF configuration options (https://www.devexpress.com/kb=T501418)
        static HRMSpaApplication()
        {
            DevExpress.Persistent.Base.PasswordCryptographer.EnableRfc2898 = true;
            DevExpress.Persistent.Base.PasswordCryptographer.SupportLegacySha512 = false;
        }
        private void InitializeDefaults()
        {
            LinkNewObjectToParentImmediately = false;
        }
        #endregion
        public HRMSpaApplication() : base()
        {
            Tracing.Initialize();
            if (ConfigurationManager.ConnectionStrings["ConnectionString"] != null)
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
            InitializeComponent();
            InitializeDefaults();
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached && CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema)
            {
                DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            }
#endif
        }

        private void HRMSpaApplication_DatabaseVersionMismatch(object sender, DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                e.Updater.Update();
                e.Handled = true;
            }
            else
            {
                string message = "The application cannot connect to the specified database, " +
                    "because the database doesn't exist, its version is older " +
                    "than that of the application or its schema does not match " +
                    "the ORM data model structure. To avoid this error, use one " +
                    "of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

                if (e.CompatibilityError != null && e.CompatibilityError.Exception != null)
                {
                    message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
                }
                throw new InvalidOperationException(message);
            }
        }

        protected override void CreateDefaultObjectSpaceProvider(CreateCustomObjectSpaceProviderEventArgs args)
        {
			XpoDataStoreProviderService xpoDataStoreProviderService;
            if(ServiceProvider != null) {
                xpoDataStoreProviderService = ServiceProvider.GetRequiredService<XpoDataStoreProviderService>();
            }
            else {
                xpoDataStoreProviderService = new XpoDataStoreProviderService();
            }
            args.ObjectSpaceProviders.Add(new XPObjectSpaceProvider(xpoDataStoreProviderService.GetDataStoreProvider(args.ConnectionString, null, true), true));
            args.ObjectSpaceProviders.Add(new NonPersistentObjectSpaceProvider(TypesInfo, null));
        }
    }
}
