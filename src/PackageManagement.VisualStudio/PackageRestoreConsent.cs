﻿using NuGet.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.PackageManagement.VisualStudio
{
    public class PackageRestoreConsent
    {
        private const string EnvironmentVariableName = "EnableNuGetPackageRestore";
        private const string PackageRestoreSection = "packageRestore";
        private const string PackageRestoreConsentKey = "enabled";

        // the key to enable/disable automatic package restore during build.
        private const string PackageRestoreAutomaticKey = "automatic";

        private readonly ISettings _settings;
        private readonly IEnvironmentVariableReader _environmentReader;
        private readonly ConfigurationDefaults _configurationDefaults;

        public PackageRestoreConsent(ISettings settings)
            : this(settings, new EnvironmentVariableWrapper())
        {
        }

        public PackageRestoreConsent(ISettings settings, IEnvironmentVariableReader environmentReader)
            : this(settings, environmentReader, ConfigurationDefaults.Instance)
        {
        }

        public PackageRestoreConsent(ISettings settings, IEnvironmentVariableReader environmentReader, ConfigurationDefaults configurationDefaults)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (environmentReader == null)
            {
                throw new ArgumentNullException("environmentReader");
            }

            if (configurationDefaults == null)
            {
                throw new ArgumentNullException("configurationDefaults");
            }

            _settings = settings;
            _environmentReader = environmentReader;
            _configurationDefaults = configurationDefaults;
        }

        public bool IsGranted
        {
            get
            {
                string envValue = PathUtility.SafeTrim(_environmentReader.GetEnvironmentVariable(EnvironmentVariableName));
                return IsGrantedInSettings || IsSet(envValue);
            }
        }

        public bool IsGrantedInSettings
        {
            get
            {
                string settingsValue = _settings.GetValue(PackageRestoreSection, PackageRestoreConsentKey);
                if (String.IsNullOrWhiteSpace(settingsValue))
                {
                    settingsValue = _configurationDefaults.DefaultPackageRestoreConsent;
                }
                settingsValue = PathUtility.SafeTrim(settingsValue);

                if (String.IsNullOrEmpty(settingsValue))
                {
                    // default value of user consent is true.
                    return true;
                }

                return IsSet(settingsValue);
            }
            set
            {
                _settings.SetValue(PackageRestoreSection, PackageRestoreConsentKey, value.ToString());
            }
        }

        public bool IsAutomatic
        {
            get
            {
                string settingsValue = _settings.GetValue(PackageRestoreSection, PackageRestoreAutomaticKey);
                if (String.IsNullOrWhiteSpace(settingsValue))
                {
                    return IsGrantedInSettings;
                }
                settingsValue = PathUtility.SafeTrim(settingsValue);
                return IsSet(settingsValue);
            }
            set
            {
                _settings.SetValue(PackageRestoreSection, PackageRestoreAutomaticKey, value.ToString());
            }
        }

        private static bool IsSet(string value)
        {
            bool boolResult;
            int intResult;
            return ((Boolean.TryParse(value, out boolResult) && boolResult) ||
                   (Int32.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out intResult) && (intResult == 1)));
        }
    }
}