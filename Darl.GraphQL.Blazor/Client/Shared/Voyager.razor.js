export function initialize(hostElement) {

    GraphQLVoyager.init(hostElement), {
        introspection: introspectionProvider
    };
}

function introspectionProvider(introspectionQuery) {
    return fetch(/*window.location.protocol + "//" + window.location.host +*/ "/graphql", {
        method: 'post',
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
    body: JSON.stringify({ query: introspectionQuery }),
        credentials: 'include',
      }).then(function (response) {
            return response.text();
        }).then(function (responseBody) {
            try {
                return JSON.parse(responseBody);
            } catch (error) {
                return responseBody;
            }
        });
    }