Imports Contensive.Addons.Blog.Controllers
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Views
    Public Class BlogClass
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
                ' -- requests model - todo the controller for each view should handle its own requests
                Dim blogBodyRequest As New BlogBodyRequestModel(CP)
                Dim legacyRequest = New View.RequestModel(CP)
                '
                ' -- get blog settings
                Dim blog As BlogModel = BlogModel.verifyBlog(CP, blogBodyRequest.instanceGuid)
                If (blog Is Nothing) Then Return "<!-- Could not find or create blog from instanceId [" & blogBodyRequest.instanceGuid & "] -->"
                '
                ' -- process view requests
                Select Case blogBodyRequest.srcViewId
                    Case Else
                        '
                        ' -- default process nothing
                End Select
                '
                ' -- create next view
                Select Case blogBodyRequest.dstViewId
                    Case Else
                        '
                        ' -- default view
                End Select
                '
                Dim app As New ApplicationEnvironmentModel(CP, blog, blogBodyRequest.entryId)
                '
                ' -- get legacy Blog Body -- the body is the area down the middle that includes the Blog View (Article View, List View, Edit View)
                Dim legacyBlogBody As String = BlogBodyView.getBlogBody(CP, app, legacyRequest, blogBodyRequest)
                '
                ' -- sidebar wrapper
                Dim blogSidebarHtml As String = SidebarView.getSidebarView(CP, app, legacyRequest, legacyBlogBody)
                '
                ' -- if editing enabled, add the link and wrapperwrapper
                Return genericController.addEditWrapper(CP, blogSidebarHtml, blog.id, blog.name, BlogModel.tableMetadata.tableMetadata.contentName)
                '
            Catch ex As Exception
                CP.Site.ErrorReport(ex, "execute")
                Return "<p>The blog is not currently available.</p>"
            End Try
        End Function

    End Class
    '
    Public Class BlogBodyRequestModel
        Public Property instanceGuid As String
        Public Property srcViewId As Integer
        Public Property dstViewId As Integer
        Public Property entryId As Integer
        '
        Public Sub New(cp As CPBaseClass)
            srcViewId = cp.Doc.GetInteger(rnFormID)
            instanceGuid = cp.Doc.GetText("instanceId")
            If (String.IsNullOrWhiteSpace(instanceGuid)) Then instanceGuid = "BlogWithoutinstanceId-PageId-" & cp.Doc.PageId
            entryId = cp.Doc.GetInteger(RequestNameBlogEntryID)
        End Sub

    End Class
End Namespace
