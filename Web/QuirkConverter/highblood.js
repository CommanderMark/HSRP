// Global let for which type of parsing (Quote, Trollian, ect.) to have the converter button output to.
let outputTextType = 0;

document.addEventListener('DOMContentLoaded', function()
{
    document.getElementById("outputText").disabled = true;
    updateTextType();
    make_copy_button(document.getElementById("outputText"));
}, true);

// Convert Array back to a String.
function arrayToString(text) {
    let result = "";
    for (let i = 0; i < text.length; i++) {
        result += text[i];
    }

    return result;
}

function getInputText()
{
    return document.getElementById("parsedText").value;
}

function output(str)
{
    switch (outputTextType)
    {
        case 1: outputTextTrollian(str); break;
        case 2: outputTextRaw(str); break;
        default: outputTextQuote(str);
    }
}

// Output types.
// Just the text.
function outputTextRaw(str)
{
    document.getElementById("outputText").innerHTML = str;
}

// With quotation marks around it.
function outputTextQuote(str)
{
    document.getElementById("outputText").innerHTML = "\"" + str + "\"";
}

// With a code-lined (`foo`) anagram in front of it.
function outputTextTrollian(str)
{
    let ana = document.getElementById("anagram").value;
    document.getElementById("outputText").innerHTML = "`" + ana + "`: " + str;
}

// Updates whether or not the output is to be a quote, a Trollian message, or just raw.
function updateTextType()
{
    outputTextType = document.getElementById("parseType").selectedIndex;

    document.getElementById("anagram").disabled = (outputTextType !== 1);
}