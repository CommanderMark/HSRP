// Grand Highblood.
function omvQuirk()
{
    let str = getInputText();

    str = str.replace(/\b(\w)/g, function(match) { return match.toUpperCase(); });
    str = str.replace(/(\w)\b/g, function(match) { return match.toUpperCase(); });

    output(str);
}

// Guild Master.
function guildMasterQuirk()
{
    let str = getInputText().toUpperCase();

    str = str.replace(/\b(\w)/g, function(match) { return match.toLowerCase(); });
    str = str.replace(/(\w)\b/g, function(match) { return match.toLowerCase(); });

    output(str);
}

// Sicarius Anathera
function sicariusQuirk()
{
    let str = getInputText();

    let indexes = [];
    let capitalIndexes = [];

    // Index the 'm' and 'M's so that 'w's converted aren't counted.
    let quirk = str.split("");


    for (let i = 0; i < quirk.length; i++)
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
    for (let i = 0; i < quirk.length; i++)
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
    let quirk = getInputText().split("");
    for (let i = 0; i < quirk.length; i++)
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
    let str = getInputText();

    str = str.replace(/b/g, "♋");
    str = str.replace(/B/g, "♋");
    str = str.replace(/p/g, "♋");
    str = str.replace(/P/g, "♋");

    output(":Cancer: " + str);
}

// Sidewind Langston
function sideQuirk()
{
    output(getInputText());
}

// Iratar
function iratarQuirk()
{
    output(">:3 " + getInputText() + " Ɛ:<");
}

// Messar Egenas
function messarQuirk()
{
    let str = getInputText();

    str = str.replace(/b/g, "6");
    str = str.replace(/B/g, "6");
    str = str.replace(/i/g, "1");
    str = str.replace(/I/g, "1");
    str = str.replace(/s/g, "2");
    str = str.replace(/S/g, "2");

    output(str);
}

// Oblitas
function oblitasQuirk()
{
    let str = getInputText();

    str = str.replace(/ /g, "_");

    output(str);
}

// The Purifier
function purifierQuirk()
{
    let str = getInputText();

    str = str.replace(/t/g, "+");
    str = str.replace(/T/g, "+");

    output("♠️ " + str + " -_-");
}