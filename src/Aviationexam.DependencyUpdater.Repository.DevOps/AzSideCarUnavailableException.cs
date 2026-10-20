using System;

namespace Aviationexam.DependencyUpdater.Repository.DevOps;

public sealed class AzSideCarUnavailableException(
    string address,
    string resourceId,
    string detail,
    Exception? innerException = null
) : Exception(
    $"AZ side car at {address} could not provide an Azure DevOps access token for resource {resourceId}: {detail}. "
    + "The Azure CLI login inside the az-side-car container is probably expired or revoked; re-login with: "
    + "docker exec -it -u app <az-side-car container> az login --use-device-code --allow-no-subscriptions",
    innerException
);
