﻿using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReference.DotNet.Sitecore.EasyLingo
{
    public static class LanguageContainer
    {

        public static IEnumerable<LanguageVersion> GetAllowedLanguageVersions(Item sitecoreItem)
        {
            var languageList = new List<LanguageVersion>();
            var allowedLanguages = GetAllowedLanguages(sitecoreItem).ToList();
            foreach (var language in sitecoreItem.Languages)
            {
                if (!allowedLanguages.Contains(language.Name, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                using (new LanguageSwitcher(language))
                {
                    var translatedItem = sitecoreItem.Database.GetItem(sitecoreItem.ID, language);
                    if (translatedItem?.Versions != null && translatedItem.Versions.Count > 0)
                    {
                        languageList.Add(CreateLanguageVersion(sitecoreItem, language, translatedItem.IsFallback ? VersionStatus.IsFallback : VersionStatus.Exists));
                    }
                    else
                    {
                        languageList.Add(CreateLanguageVersion(sitecoreItem, language, VersionStatus.None));
                    }
                }
            }

            return languageList.OrderBy(l => l.Name).ToList();
        }

        public static IEnumerable<LanguageVersion> GetDisallowedLanguageVersions(Item sitecoreItem)
        {
            var languageList = new List<LanguageVersion>();
            var allowedLanguages = GetAllowedLanguages(sitecoreItem).ToList();
            foreach (var language in sitecoreItem.Languages)
            {
                if (allowedLanguages.Contains(language.Name, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                using (new LanguageSwitcher(language))
                {
                    var translatedItem = sitecoreItem.Database.GetItem(sitecoreItem.ID, language);
                    if (translatedItem?.Versions != null && translatedItem.Versions.Count > 0)
                    {
                        //TODO check: Is the isFallback check still needed here?
                        languageList.Add(CreateLanguageVersion(sitecoreItem, language, translatedItem.IsFallback ? VersionStatus.IsFallback : VersionStatus.Exists));
                    }
                }
            }

            return languageList.OrderBy(l => l.Name).ToList();
        }
        
        private static LanguageVersion CreateLanguageVersion(Item currentItem, Language language, VersionStatus status)
        {
            var languageItem = language.Origin != null && !ID.IsNullOrEmpty(language.Origin.ItemId) ? currentItem.Database.GetItem(language.Origin.ItemId) : null;
            var languageName = languageItem != null ? languageItem.Name : language.Name;
            var iconUrl = LanguageContainer.GetIconUrl(languageItem);

            return new LanguageVersion
            {
                Name = languageName,
                Status = status,
                Icon = iconUrl,
                Origin = language.Origin.ItemId
            };
        }


        private static IEnumerable<string> GetAllowedLanguages(Item currentItem)
        {
            var sites = GetSites(currentItem);
            var result = new List<string>();
            foreach (var site in sites)
            {
                var allowedLanguages = site.Properties[LanguageResolver.LanguagesProperty];
                if (!string.IsNullOrEmpty(allowedLanguages))
                {
                    result.AddRange(allowedLanguages.Split(','));
                }
            }

            result = result.Any() ? result : LanguageManager.GetLanguages(currentItem.Database).Select(l => l.Name).ToList<string>();
            return result.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<SiteInfo> GetSites(Item item)
        {
            return global::Sitecore.Configuration.Factory.GetSiteInfoList().Where(s => !string.IsNullOrEmpty(s.RootPath) && item.Paths.FullPath.StartsWith(s.RootPath));
        }

        public static string GetIconUrl(Item languageItem)
        {
            if(languageItem == null)
            {
                return "/temp/IconCache/Office/16x16/flag_generic.png";
            }
            string DefaultLanguageIconPath = "Network/16x16/earth.png";
            var iconPath = languageItem != null ? languageItem.Appearance.Icon : DefaultLanguageIconPath;
            iconPath = iconPath.Replace("24x24", "16x16").Replace("32x32", "16x16").Replace("48x48", "16x16");
            return $"/temp/IconCache/{iconPath}";
        }
    }
}
