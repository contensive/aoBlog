
Imports Contensive.Models.Db

Namespace Models
    Public Class BlogViewingLogModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Viewing Log", "BlogViewingLog", "default", False)
        '
        '====================================================================================================
        ' -- instance properties
        'instancePropertiesGoHere
        Public Property BlogEntryID As Integer
        Public Property MemberID As Integer
        Public Property VisitID As Integer
    End Class
End Namespace
