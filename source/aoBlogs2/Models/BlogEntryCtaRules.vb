
Imports Contensive.Models.Db

Namespace Models
    Public Class BlogEntryCTARuleModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Entry CTA Rules", "ccBlogEntryCTARules", "default", False)
        '
        '====================================================================================================
        ' -- instance properties
        Public Property blogentryid As Integer
        Public Property calltoactionid As Integer
    End Class
End Namespace
