Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    Public Class BlogCategoriesClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        ''' <summary>
        ''' Blog Addon
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Try
                '
                ' -- change 'blog Authors' to standard group 'staff
                Dim staffGroup As GroupModel = DbModel.create(Of GroupModel)(CP, guidStaffGroup)
                If staffGroup Is Nothing Then
                    staffGroup = DbModel.add(Of GroupModel)(CP)
                    staffGroup.name = "Staff"
                    staffGroup.Caption = "Staff"
                    staffGroup.ccguid = guidStaffGroup
                End If
                CP.Utils.
                '
                '
                ' -- delete legacy blog
                CP.Db.ExecuteNonQuery("delete from ccaggregatefunctions where ccguid='{656E95EA-2799-45CD-9712-D4CEDF0E2D02}'")
                '
                ' -- delete blog categores list
                CP.Db.ExecuteNonQuery("delete from ccaggregatefunctions where ccguid='{534B4D50-B894-42BC-AB42-7AF6EC6265DE}'")
                '
                ' -- setup blog entry author select - not installing correctly
                CP.Db.ExecuteNonQuery("update ccfields set MemberSelectGroupID=" & CP.Content.GetRecordID("groups", "Site Managers") & " where name='authormemberid'")
                '
                Return ""
            Catch ex As Exception
                CP.Site.ErrorReport(ex, "execute")
                Throw
            End Try
        End Function
    End Class
End Namespace
