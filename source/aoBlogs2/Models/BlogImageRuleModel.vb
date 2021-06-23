
Imports Contensive.Models.Db

Namespace Models
    Public Class BlogImageRuleModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Image Rules", "BlogImageRules", "default", False)
        '
        '====================================================================================================
        ' -- instance properties
        'instancePropertiesGoHere
        Public Property BlogEntryID As Integer
        Public Property BlogImageID As Integer


    End Class
End Namespace
