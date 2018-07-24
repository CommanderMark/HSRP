// Sayana Cantet
function sayanaQuirk()
{
    let str = getInputText();

    str = str.replace(/(e)/gi, "-$1");
    str = str.replace(/o/gi, "()");

    output(str);
}

// The Purifier
function purifierQuirk()
{
    let str = getInputText();

    str = str.replace(/t/gi, "+");

    output("♠️ " + str + " -_-");
}

// Keahim Shradu
function keahimQuirk()
{
    output(":Aquarius: " + getInputText());
}

// Melissa
function melQuirk()
{
    let str = getInputText();

    str = str.replace(/y/g, "λ");
    str = str.replace(/Y/g, "Λ");

    output(str + " :Aquarius:");
}

// Hunter
function hunterQuirk()
{
    let str = getInputText();

    str = str.replace(/d/gi, "Đ");

    output(":Sagittarius: " + str);
}