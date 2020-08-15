// The address of the ThemeController API, the one setting the sites need
var serverUrl = '/Themes/';

function requestToServer(action, method, data, onSuccess) {
    var postData = method == 'GET' ? '' : data;
    $.ajax({
        url: serverUrl + action,
        method: method,
        contentType: 'application/json;charset=utf-8',
        accept: 'application/json',
        data: JSON.stringify(postData),
        crossDomain: true,
        headers: {
            'Content-Type': 'application/json',
            'Access-Control-Allow-Origin': '*'
        },
        error: function () {
            showMessage('Unknown error');
        },
        success: function (result) {
            if (result && typeof onSuccess == 'function') {
                onSuccess(result)
            } else if (result === false) {
                showMessage('Failed');
            }
        }
    });
}

function showMessage(message) {
    alert(message);
}