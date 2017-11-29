// Global var for which type of parsing (Quote, Trollian, ect.) to have the converter button output to.
var outputTextType = 0;

document.addEventListener('DOMContentLoaded', function()
{
    document.getElementById("outputText").disabled = true;
    updateTextType();
});

// Grand Highblood.
function omvQuirk()
{
    var str = getInputText();

    str = str.replace(/\b(\w)/g, function(match) { return match.toUpperCase(); });
    str = str.replace(/(\w)\b/g, function(match) { return match.toUpperCase(); });

    output(str);
}

// Sicarius
function sicariusQuirk()
{
    var str = getInputText();

    var indexes = [];
    var capitalIndexes = [];
    // Index the 'm' and 'M's so that 'w's converted aren't counted.
    var quirk = str.split("");
    for (var i = 0; i < quirk.length; i++)
    {
        if (quirk[i] === "m")
        {
            indexes.push(i);
        }
        else if (quirk[i] === "M")
        {
            capitalIndexes.push(i);
        }
    }

    str = str.replace(/w/g, "m");
    str = str.replace(/W/g, "M");

    quirk = str.split("");
    for (var i = 0; i < quirk.length; i++)
    {
        if (indexes.includes(i))
        {
            quirk[i] = "w";
        }
        else if (capitalIndexes.includes(i))
        {
            quirk[i] = "W";
        }
    }

    str = arrayToString(quirk);
    str = str.replace(/a/g, "8");
    str = str.replace(/A/g, "8");
    str = str.replace(/b/g, "8");
    str = str.replace(/B/g, "8");

    output(str);
}

// HECTOR??? HE WAS THE RAT??
function hectorQuirk()
{
    var quirk = getInputText().split("");
    for (var i = 0; i < quirk.length; i++)
    {
        if (quirk[i] === "i")
        {
            quirk[i] = "iii";
        }
        else if (quirk[i] === "I")
        {
            quirk[i] = "III";
        }
        else if (quirk[i] === "!")
        {
            quirk[i] = "!!!";
        }
    }

    output(arrayToString(quirk));
}

function vanderQuirk()
{
    var str = getInputText();

    str = str.replace(/b/g, ":cancer:");
    str = str.replace(/B/g, ":cancer:");
    str = str.replace(/p/g, ":cancer:");
    str = str.replace(/P/g, ":cancer:");

    output(":Cancer: " + str);
}

// Sidewind
function sideQuirk()
{
    output(getInputText());
}

// Convert Array back to a String.
function arrayToString(text) {
    var result = "";
    for (var i = 0; i < text.length; i++) {
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
    document.getElementById("outputText").value = str;
}

// With quotation marks around it.
function outputTextQuote(str)
{
    document.getElementById("outputText").value = "\"" + str + "\"";
}

// With a code-lined (`foo`) anagram in front of it.
function outputTextTrollian(str)
{
    var ana = document.getElementById("anagram").value;
    document.getElementById("outputText").value = "`" + ana + "`: " + str;
}

// Updates whether or not the output is to be a quote, a Trollian message, or just raw.
function updateTextType()
{
    outputTextType = document.getElementById("parseType").selectedIndex;

    document.getElementById("anagram").disabled = (outputTextType !== 1);
}

function copyToClipboard()
{
    if (navigator.userAgent.match(/ipad|ipod|iphone/i))
    {

    }
    else
    {
        var copyTextArea = document.querySelector("#outputText");
        copyTextArea.select();

        var successful = document.execCommand('copy');
    }
}