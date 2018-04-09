<?php
class Quirk
{
    // String
    public $name;

    // String
    public $prefix;
    // String
    public $suffix;

    // Array
    public $search;
    // Array
    public $replace;

    function stro_replace($search, $replace, $subject)
    {
        return strtr($subject, array_combine($search, $replace));
    }

    function parseText($str)
    {
        $newStr = $str;
        if (isset($this->search) && isset($this->replace))
        {
            $newStr = $this->stro_replace($this->search, $this->replace, $newStr);
        }

        if (isset($this->prefix))
        {
            
            $newStr = $this->prefix . $newStr;
        }

        
        if (isset($this->suffix))
        {
            $newStr = $newStr . $this->suffix;
        }

        return $newStr;
    }
}

$bloin = new Quirk;
$bloin->suffix = " suffix";
$bloin->prefix = "prefix ";
$bloin->search = array('o', 'j');
$bloin->replace = array('j', 'o');
$str = "the lazy fox jumped quickly over the dog";

echo $bloin->parseText($str);
?>

<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Quirks for some RP server idk</title>
    <script src="script.js"></script>
    <script src="copy2clipboard.js"></script>
    <link rel="stylesheet" href="style.css">
</head>
<body>

<div class="headerTitle">Enter stuff:</div>
 <!-- TODO: Point to script .  -->
<form method="get" action="yourFileName.php">
    <table>
        <tr>
            <td><label for="anagram">Anagram for Trollian (optional):</label></td>
            <td><input type="text" id="anagram" name="anagram"></td>
        </tr>

        <tr>
            <td><label for="parsedText">Text to parse:</label></td>
            <td><textarea id="parsedText" name="parsedText"></textarea></td>
        </tr>

        <tr>
            <td><label for="parseType">Format of Output:</label></td>
            <td>
                <select id="parseType" onchange="updateTextType()" name="parseType">
                    <option value="1">Quote</option>
                    <option value="2">Trollian</option>
                    <option value="3">Raw</option>
                </select>
            </td>
        </tr>
    </table>
</form>

<table class="quirkCol">
    <tr><td><label>Mains:</label></td></tr>
    <tr><td><input type="submit" value="Sicarius" onclick="sicariusQuirk()" /></td>
    <tr><td><input type="submit" value="Sidewind" onclick="sideQuirk()" /></td></tr>
</table>

<table class="quirkCol">
    <tr><td><label>Alts:</label></td></tr>
    <tr><td><input type="submit" value="Iratar" onclick="iratarQuirk()" /></td></tr>
    <tr><td><input type="submit" value="Messar" onclick="messarQuirk()" /></td></tr>
    <tr><td><input type="submit" value="Oblitas" onclick="oblitasQuirk()" /></td></tr>
    <tr><td><input type="submit" value="Purifier" onclick="purifierQuirk()" /></td></tr>
    <tr><td><input type="submit" value="Guild Master" onclick="guildMasterQuirk()" /></td></tr>
</table>

<table class="quirkCol">
    <tr><td><label>Signless:</label></td></tr>
    <tr><td><input type="submit" value="Hector" onclick="hectorQuirk()" /></td></tr>
    <tr><td><input type="submit" value="Vander" onclick="vanderQuirk()" /></td></tr>
</table>

<table class="quirkCol">
    <tr><td><label>Unused(?):</label></td></tr>
    <tr><td><input type="submit" value="OMV" onclick="omvQuirk()" /></td></tr>
</table>

<div style="clear:both;"></div>

<table>
    <tr><td><label for="outputText">Result:</label></td></tr>
    <tr><td><div onclick="select_all_and_copy(this)" id="outputText"></div></td></tr>
</table>

</body>
</html>