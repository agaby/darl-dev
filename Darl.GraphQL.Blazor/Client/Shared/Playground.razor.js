export function initialize(hostElement) {
//    window.renderReactApp = function () {
//        ReactDOM.render('<App />', hostElement);
//    }


        GraphQLPlayground.init(hostElement,
            {
                setTitle: true,
                endpoint: "/graphql",
                subscriptionEndpoint: getSubscriptionsEndPoint(),
                config: "",
                settings: "",
                headers: ""
            });
}




function getSubscriptionsEndPoint() {
    let subscriptionsEndPoint = "/graphql";
    if (/^(?:[a-z]+:)?\/\//i.test(subscriptionsEndPoint)) {
        // if location includes protocol (e.g. "wss://") then return exact string
        return subscriptionsEndPoint;
    } else if (subscriptionsEndPoint[0] != '/') {
        // if location is relative (e.g. "api") then prepend host and current path
        let currentUrl = /^[^?]*(?=\/)/.exec(window.location.pathname);
        currentUrl = currentUrl ? currentUrl[0] : '';
        while (subscriptionsEndPoint.substring(0, 3) == '../') {
            subscriptionsEndPoint = subscriptionsEndPoint.substring(3);
            currentUrl = /^[^?]*(?=\/)/.exec(currentUrl);
            currentUrl = currentUrl ? currentUrl[0] : '';
        }
        return (window.location.protocol === "http:" ? "ws://" : "wss://") + window.location.host + currentUrl + '/' + subscriptionsEndPoint;
    }
    // if location is absolute (e.g. "/api") then prepend host only
    return (window.location.protocol === "http:" ? "ws://" : "wss://") + window.location.host + subscriptionsEndPoint;
}

