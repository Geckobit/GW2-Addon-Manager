﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using GW2_Addon_Manager.App.Configuration;

namespace GW2_Addon_Manager
{
    class UpdateHelpers
    {
        public static dynamic GitReleaseInfo(string gitUrl)
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", "request");
            try
            {
                string release_info_json = client.DownloadString(gitUrl);
                return JsonConvert.DeserializeObject(release_info_json);

            }
            catch (WebException)
            {
                //TODO: Add this catch to API calls made at application startup as well
                MessageBox.Show("Github Servers returned an error; please try again in a few minutes.", "Github API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SelfUpdate.startUpdater();
                Application.Current.Shutdown();
                return null;
            }
            
        }

        public static async void UpdateAll()
        {
            UpdatingViewModel viewModel = UpdatingViewModel.GetInstance;

            LoaderSetup settingUp = new LoaderSetup(new ConfigurationManager());
            await settingUp.HandleLoaderUpdate();

            List<AddonInfoFromYaml> addons = (List<AddonInfoFromYaml>)Application.Current.Properties["Selected"];
            
            var configurationManager = new ConfigurationManager();
            foreach (AddonInfoFromYaml addon in addons.Where(add => add != null))
            {
                GenericUpdater updater = new GenericUpdater(addon, configurationManager);
            
                if(!(addon.additional_flags != null && addon.additional_flags.Contains("self-updating") 
                     && configurationManager.UserConfig.AddonsList.FirstOrDefault(a => a.Name == addon.addon_name)?.Installed == true))
                    await updater.Update();
            }

            viewModel.ProgBarLabel = "Updates Complete";
            viewModel.DownloadProgress = 100;
            viewModel.CloseBtnEnabled = true;
            viewModel.BackBtnEnabled = true;
        }
    }
}
