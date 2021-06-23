
Imports Contensive.Models.Db

Namespace Models
    Public Class CallsToActionModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Calls To Action", "callsToAction", "default", False)
        '
        '====================================================================================================
        '
        ' -- instance properties
        Public Property link As String
        Public Property headline As String
        Public Property brief As String
    End Class
End Namespace
