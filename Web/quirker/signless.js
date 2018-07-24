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


// Vander
function vanderQuirk()
{
    let str = getInputText();

    str = str.replace(/b/g, "♋");
    str = str.replace(/B/g, "♋");
    str = str.replace(/p/g, "♋");
    str = str.replace(/P/g, "♋");

    output(":Cancer: " + str);
}