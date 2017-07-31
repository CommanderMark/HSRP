function quirk1() {
    var str = document.getElementById("text").value;
    str = applyQuirk(str);

    document.getElementById("output").innerHTML = str;
}

function quirk2() {
    var str = document.getElementById("text").value;
    str = applyQuirk(str);

    document.getElementById("output").innerHTML = "`OMV`: " + str;
}

function applyQuirk(str) {
    str = str.replace(/\b(\w)/g, function(match) { return match.toUpperCase(); });
    str = str.replace(/(\w)\b/g, function(match) { return match.toUpperCase(); });

    return str;
}