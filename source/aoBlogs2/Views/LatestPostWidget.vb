Imports Contensive.BaseClasses

Public Class LatestPostWidget
    Inherits AddonBaseClass

    Public Overrides Function Execute(ByVal cp As CPBaseClass) As Object
        Try
            Dim widgetName As String = "Latest News Widget"
            Return DesignBlockController.renderWidget(Of DbLatestNewsWidgetsModel, LatestNewsWidgetViewModel)(cp, widgetName:=widgetName, layoutGuid:="{cf55cad2-a7a4-4c74-8ffe-4327e3174372}", layoutName:="News Home Layout", layoutPathFilename:="Watf\index.html", layoutBS5PathFilename:="Watf\index.html")
        Catch ex As Exception
            cp.Site.ErrorReport(ex)
            Return ""
        End Try
    End Function
End Class
