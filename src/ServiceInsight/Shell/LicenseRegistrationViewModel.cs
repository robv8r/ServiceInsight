﻿namespace ServiceInsight.Shell
{
    using System.IO;
    using Caliburn.Micro;
    using ServiceInsight.Framework;
    using ServiceInsight.Framework.Licensing;
    using ServiceInsight.Framework.UI.ScreenManager;

    public class LicenseRegistrationViewModel : Screen
    {
        AppLicenseManager licenseManager;
        IWindowManagerEx dialogManager;
        NetworkOperations network;

        public LicenseRegistrationViewModel(
            AppLicenseManager licenseManager,
            IWindowManagerEx dialogManager,
            NetworkOperations network)
        {
            this.licenseManager = licenseManager;
            this.dialogManager = dialogManager;
            this.network = network;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ValidationResult = null;
            DisplayName = GetScreenTitle();
        }

        string GetScreenTitle()
        {
            var expired = licenseManager.CurrentLicense.HasExpired();

            if (licenseManager.CurrentLicense.IsCommercialLicense)
            {
                if (expired)
                {
                    return "ServiceInsight - License Expired";
                }
                return "ServiceInsight";
            }
            if (HasRemainingTrial)
            {
                return string.Format("ServiceInsight - {0} day(s) left on your free trial", TrialDaysRemaining);
            }
            return string.Format("ServiceInsight - {0} Trial Expired", TrialTypeText);
        }

        public string TrialTypeText => licenseManager.CurrentLicense.IsExtendedTrial ? "Extended" : "Initial";

        public string LicenseType => licenseManager.CurrentLicense.LicenseType;

        public string RegisteredTo => licenseManager.CurrentLicense.RegisteredTo;

        public int TrialDaysRemaining => licenseManager.GetRemainingTrialDays();

        public bool HasTrialLicense => licenseManager.CurrentLicense.IsTrialLicense;

        public bool HasFullLicense => licenseManager.CurrentLicense.IsCommercialLicense;

        public bool HasRemainingTrial => HasTrialLicense && TrialDaysRemaining > 0;

        public bool AllowedToUse => HasRemainingTrial || HasFullLicense;

        public bool CanExtendTrial => HasTrialLicense && !licenseManager.CurrentLicense.IsExtendedTrial;

        public bool CanBuyNow => HasTrialLicense && licenseManager.CurrentLicense.IsExtendedTrial;

        public bool MustExtendTrial => HasTrialLicense && !HasRemainingTrial && !licenseManager.CurrentLicense.IsExtendedTrial;

        public bool MustPurchase => HasTrialLicense && !HasRemainingTrial && licenseManager.CurrentLicense.IsExtendedTrial;

        public bool ShowValidationErrors => ValidationResult != null;

        public string ValidationResult { get; set; }

        public void OnLicenseChanged()
        {
            NotifyOfPropertyChange(nameof(LicenseType));
            NotifyOfPropertyChange(nameof(RegisteredTo));
            NotifyOfPropertyChange(nameof(TrialDaysRemaining));
            NotifyOfPropertyChange(nameof(ShowValidationErrors));
        }

        public void LoadLicense()
        {
            var dialog = dialogManager.OpenFileDialog(new FileDialogModel
            {
                Filter = "License files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 1
            });

            var validLicense = false;
            ValidationResult = null;

            if (dialog.Result.GetValueOrDefault(false))
            {
                var licenseContent = ReadAllTextWithoutLocking(dialog.FileName);

                validLicense = licenseManager.TryInstallLicense(licenseContent) == LicenseInstallationResult.Succeeded;
            }

            if (validLicense && !licenseManager.CurrentLicense.HasExpired())
            {
                TryClose(true);
            }
            else
            {
                ValidationResult = licenseManager.ValidationResult.Result;
            }
        }

        public void Close()
        {
            TryClose(AllowedToUse);
        }

        public void Purchase()
        {
            network.Browse("http://particular.net/licensing");
        }

        public void Extend()
        {
            network.Browse("http://particular.net/extend-your-trial-14");
        }

        public void ContactSales()
        {
            network.Browse("mailto:contact@particular.net?subject=ServiceInsight");
        }

        string ReadAllTextWithoutLocking(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var textReader = new StreamReader(fileStream))
            {
                return textReader.ReadToEnd();
            }
        }
    }
}