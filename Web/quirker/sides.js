// Iratar
function iratarQuirk()
{
    output(">:3 " + getInputText() + " Ɛ:<");
}

// Messar Egenas
function messarQuirk()
{
    let str = getInputText();

    str = str.replace(/b/gi, "6");
    str = str.replace(/i/gi, "1");
    str = str.replace(/s/gi, "2");

    output(str);
}

// Kordra Fyrenn
function kordraQuirk()
{
    let str = getInputText();

    let indexes = [];
    let capitalIndexes = [];

    // Index the 'm' and 'M's so that 'w's converted aren't counted.
    let quirk = str.split("");

    for (let i = 0; i < quirk.length; i++)
    {
        if (quirk[i] === "i")
        {
            indexes.push(i);
        }
        else if (quirk[i] === "I")
        {
            capitalIndexes.push(i);
        }
    }

    str = str.replace(/l/gi, "I");

    quirk = str.split("");
    for (let i = 0; i < quirk.length; i++)
    {
        if (indexes.includes(i))
        {
            quirk[i] = "l";
        }
        else if (capitalIndexes.includes(i))
        {
            quirk[i] = "L";
        }
    }

    str = arrayToString(quirk);
    str = str.replace(":)", "]:3")

    output(str);
}

// Oblitas
function oblitasQuirk()
{
    let str = getInputText();

    str = str.replace(/ /g, "_");

    output(str);
}

// Ryvern
function ryvernQuirk()
{
    output("_" + getInputText() + "_");
}

// Juggalo
function juggaloQuirk()
{
    let str = getInputText();

    str = str.replace(/\b(\w)/g, function(match) { return match.toUpperCase(); });
    str = str.replace(/(\w)\b/g, function(match) { return match.toUpperCase(); });

    output(str);
}
