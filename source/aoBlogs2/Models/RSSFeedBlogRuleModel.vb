

Imports Contensive.Models.Db

Namespace Models
    Public Class RSSFeedBlogRuleModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("RSS Feed Blog Rules", "ccRSSFeedRules", "default", False)
        '
        '====================================================================================================
        ' -- instance properties
        'instancePropertiesGoHere
        Public Property BlogPostID As Integer
        Public Property RSSFeedID As Integer

    End Class
End Namespace
