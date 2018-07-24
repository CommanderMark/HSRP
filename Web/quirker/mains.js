// Sicarius Anathera
function sicariusQuirk()
{
    let str = getInputText();

    // All the shit below is Strana's quirk. Kill me.
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

    // Sicarius' quirk.
    str = str.replace(/a/gi, "8");
    str = str.replace(/b/gi, "8");

    output(str);
}

// Sidewind Langston
function sideQuirk()
{
    let str = getInputText();

    str = str.replace(/p/g, "q");
    str = str.replace(/P/g, "Q");

    output(str);
}

// Default
function defaultQuirk()
{
    output(getInputText());
}