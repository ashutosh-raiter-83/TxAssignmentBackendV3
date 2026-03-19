using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Server.Swagger;

public class ApiExplorerGroupConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNamespace = controller.ControllerType.Namespace;
        if (controllerNamespace == null)
        {
            controller.ApiExplorer.GroupName = "top-level";
            return;
        }

        var parts = controllerNamespace.Split('.');

        if (parts.Contains("RobotUser"))
        {
            controller.ApiExplorer.GroupName = "robot-user";
        }
        else if (parts.Contains("TechnicianUser"))
        {
            controller.ApiExplorer.GroupName = "technician-user";
        }
        else
        {
            controller.ApiExplorer.GroupName = "top-level";
        }
    }
}