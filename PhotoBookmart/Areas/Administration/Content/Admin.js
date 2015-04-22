/// Administration Panel Javascript
// Travel To Go project
// Copyright: Trung Dang (trungdt@absoft.vn)

var mainList_Website = [];

jQuery(document).ready(function () {

});

/// This function will reload the Main List Website on the Sidebar
function mainList_ReloadWebsite(dis_id, callback) {
    // reload 
    var html = "";
    html += "<optgroup label='Active'>";
    for (var i = 0; i < mainList_Website.length; i++) {
        var item = mainList_Website[i];
        if (item.DisId != dis_id || item.Status != 1) {
            continue;
        }
        html += "<option value=" + item.Id + ">" + item.Name + "</option>";
    }
    html += "</optgroup>";

    html += "<optgroup label='Non-active'>";
    for (var i = 0; i < mainList_Website.length; i++) {
        var item = mainList_Website[i];
        if (item.DisId != dis_id || item.Status == 1) {
            continue;
        }
        html += "<option value=" + item.Id + ">" + item.Name + "</option>";
    }
    html += "</optgroup>";

    jQuery("#MainList_Website").select2("destroy").html(html).select2();

    mainList_WebsiteChange();

    if (callback != null) {
        callback();
    }
}

function site_reload(wait) {
    if (wait == null) {
        window.location.href = window.location.href;
    }
    else {
        setTimeout(function () {
            window.location.href = window.location.href;
        }, wait);
    }
}

// Parse working time from int to hh:mm
function WorkingTimeParse(t) {
    var h = Math.floor(t / 60);
    var m = t % 60;
    var pmam = "AM";
    if (h > 11) {
        h = h - 12;
        pmam = "PM";
    }

    if (h < 10) {
        h = "0" + h;
    }
    if (m < 10) {
        m = "0" + m;
    }

    return ret = h + ":" + m + " " + pmam;
}

function getUrlParameter(_param, _url) {
    var sPageURL = window.location.search.substring(1);
    if (typeof (_url) !== "undefined") {
        var index = _url.indexOf("?");
        sPageURL = index != -1 ? _url.substring(index + 1) : "";
    }
    var sURLVariables = sPageURL.split("&");
    for (var i = 0; i < sURLVariables.length; i++) {
        var sParameterName = sURLVariables[i].split("=");
        if (sParameterName[0] == _param) {
            return sParameterName[1];
        }
    }
};
