﻿using Contensive.BaseClasses;
using Contensive.DesignBlockBase.Models.Db;
using Contensive.Models.Db;
using System;

namespace Contensive.Blog.Models.Db {
    public class LatestPostsWidgetModel : SettingsBaseModel {
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Latest Posts Widgets", "LatestPostWidgets", "default", false);
        public string defaultpostimage { get; set; }
        public static new LatestPostsWidgetModel createOrAddSettings(CPBaseClass cp, string settingsGuid, string recordNameSuffix) {
            try {
                var result = create<LatestPostsWidgetModel>(cp, settingsGuid);
                if (result != null) { return result; }
                // 
                // -- create default content
                result = addDefault<LatestPostsWidgetModel>(cp);
                result.name = $"{tableMetadata.contentName} {result.id}, created {DateTime.Now}" + (string.IsNullOrEmpty(recordNameSuffix) ? "" : ", " + recordNameSuffix);
                result.ccguid = settingsGuid;
                result.save(cp);
                // 
                // -- track the last modified date
                cp.Content.LatestContentModifiedDate.Track(result.modifiedDate);

                return result;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport("Error in DbLatestNewsWidgetsModel createOrAddSettings: " + ex);
                return new LatestPostsWidgetModel();
            }
        }
    }
}
