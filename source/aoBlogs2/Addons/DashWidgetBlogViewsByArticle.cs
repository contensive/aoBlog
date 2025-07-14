using Contensive.BaseClasses;
using Contensive.Processor.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Contensive.Blog {
    public class DashWidgetBlogViewsByArticle : AddonBaseClass {
        // Fix for CA1861: Use static readonly for the array to avoid recreating it repeatedly  
        private static readonly double[] DefaultDataValueList = [45, 20, 10, 8, 5, 4, 3, 2, 2, 1, 0];

        private static readonly DashboardWidgetBarChartModel_DataSets[] DefaultDataetsValues = {
            new() {
                    backgroundColor = "rgba(255, 99, 132, 0.2)",
                    borderColor = "rgba(255, 99, 132, 1)",
                    borderWidth = 1,
                    label = "",
                    data = [45, 20, 10, 8, 5, 4, 3, 2, 2, 1, 0]
           }
        };

        public override object Execute(CPBaseClass cp) {
            try {
                int mode = cp.Doc.GetInteger("widgetFilter");
                if (mode < 1 || mode > 3) { mode = 1; }
                //
                // -- get captions and values
                string sql = "";
                switch (mode) {
                    case 2:
                        //
                        // Lowest 10
                        sql = $@"
                            select top 10 
                                count(v.id) as cnt,b.name as post
                            from
	                            BlogViewingLog v
	                            left join ccBlogCopy b on b.id=v.blogentryid
                            group by
	                            b.name, b.id
                            having
	                            b.id is not null
                            order by count(v.id)
                        ";
                        break;
                    case 3:
                        //
                        // Last 10
                        sql = $@"
                            select top 10 
                                count(v.id) as cnt,b.name as post
                            from
                                BlogViewingLog v
                                left join ccBlogCopy b on b.id=v.blogentryid
                            group by
                                b.name, b.id, b.datepublished
                            having
                                b.id is not null
                            order by b.datepublished desc
                        ";
                        break;
                    default: 
                        //
                        // Top 10
                        sql = $@"
                            select top 10 
                                count(v.id) as cnt,b.name as post
                            from
	                            BlogViewingLog v
	                            left join ccBlogCopy b on b.id=v.blogentryid
                            group by
	                            b.name, b.id
                            having
	                            b.id is not null
                            order by count(v.id) desc
                        ";
                        break;
                };
                string[] dataLabels = [];
                double[] dataValues = [];
                int count = 0;
                using (DataTable dt = cp.Db.ExecuteQuery(sql)) {
                    if (dt?.Rows != null) {
                        count = dt.Rows.Count;
                        dataLabels = new string[count];
                        dataValues = new double[count];
                        //
                        // -- get data labels
                        for (int i = 0; i < count; i++) {
                            dataLabels[i] = cp.Utils.EncodeHTML(dt.Rows[i]["post"].ToString());
                            dataValues[i] = Convert.ToDouble(dt.Rows[i]["cnt"]);
                        }
                    }
                }
                //
                DashboardWidgetBarChartModel_DataSets[] dataSets = {
                        new() {
                                label = "Blog Posts",
                                data = dataValues.ToList()
                          }
                    };
                //
                DashboardWidgetBarChartModel result = new() {
                    widgetName = "Blog Post Views",
                    subhead = "Blog Post Views",
                    description = "Blog posts by article.",
                    uniqueId = cp.Utils.GetRandomString(4),
                    width = 2,
                    refreshSeconds = 0,
                    url = "",
                    dataLabels = dataLabels.ToList(),
                    dataSets = dataSets.ToList(),
                    widgetType = WidgetTypeEnum.bar,
                    filterOptions = new List<DashboardWidgetBaseModel_FilterOptions>() {
                           new() {
                               filterCaption = "Top 10",
                               filterValue = "1",
                               filterActive = (mode == 2)
                           },
                           new() {
                               filterCaption = "Lowest 10",
                               filterValue = "2",
                               filterActive = (mode == 6)
                           },
                           new() {
                               filterCaption = "Last 10",
                               filterValue = "3",
                               filterActive = (mode == 10)
                           }
                       }
                };
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
