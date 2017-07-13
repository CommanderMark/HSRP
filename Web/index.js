BloodType = [
    "Burgundy",
    "Bronze",
    "Yellow",
    "Lime",
    "Olive",
    "Jade",
    "Teal",
    "Cerulean",
    "Indigo",
    "Purple",
    "Violet",
    "Fuchsia"
];

window.onload = function onLoad() {
    var dropdown = document.getElementById("bloodColor")

    for (var i = 0; i < BloodType.length; i++) {
        dropdown.options[i] = new Option(BloodType[i], i);
    }
}

function generateXML() {
    var xw = new XMLWriter();
    xw.writeStartDocument();
    xw.writeStartElement('items');
    xw.writeAttributeString('the dualies', '"""');
    xw.writeEndElement();
    xw.writeEndDocument();
    alert(xw.flush());
}