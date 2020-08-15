(function () {
    requestToServer('GetDefaultTheme', 'GET', '', initializePage);
    requestToServer('GetThemesList', 'GET', '', fillThemesList);
})();

function initializePage(data) {
    let keys = Object.keys(data);
    let html = '';
    for (let index = 0; index < keys.length; index++) {
        const element = keys[index];
        if (index % 2 == 0) {
            html += '<div class="controls-group">';
        }
        html += '<div class="control-div">';
        html += '<label>' + element + '</label>';
        html += '<input class="input-control" data-key="' + element + '" type="text" placeholder="' + element + '" value="' + data[element] + '">';
        html += '</div>';

        if (index % 2 != 0) {
            html += '</div>';
        };
    }

    $('#themeContainer').empty().append(html);
}

function resetFields() {
    $('#themeContainer input').val('');
}

function addTheme() {
    let obj = readFields();
    requestToServer('AddNewTheme', 'POST', obj, function () {
        resetFields();
        requestToServer('GetThemesList', 'GET', '', fillThemesList);
        showMessage('Done');
    });
}

function readFields() {
    let resultObject = {};
    $('#themeContainer input').each(function (index) {
        const element = $(this);
        resultObject[element.attr('data-key')] = element.val();
    });
    return resultObject;
}

function fillThemesList(result) {
    let html = '';
    for (let index = 0; index < result.length; index++) {
        const element = result[index];
        html += '<option value="' + element.id + '"' + (element.isSelected ? 'selected' : '') + '>' + element.name + '</option>';
    }
    $('#theme-list').empty().append(html).trigger('change');
}

function changeTheme() {
    let selectedTheme = $('#theme-list').val();
    if (selectedTheme) {
        requestToServer('GetThemeUrl?ThemeId=' + selectedTheme, 'GET', '', updateTheme);
    }
}

function updateTheme(url) {
    $('#theme-css-link').attr('href', url);
}