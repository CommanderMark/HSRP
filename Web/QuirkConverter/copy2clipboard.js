/* Cool Javascript Copy to Clipboard Crossbrowser
Version 1.1
Written by Jeff Baker on March 18, 2016
Copyright 2016 by Jeff Baker -
http://www.seabreezecomputers.com/tips/copy2clipboard.htm
*/

function tooltip(el, message)
{
    let scrollLeft = document.body.scrollLeft || document.documentElement.scrollLeft;
    let scrollTop = document.body.scrollTop || document.documentElement.scrollTop;
    let x = parseInt(el.getBoundingClientRect().left.toString()) + scrollLeft + 10;
    let y = parseInt(el.getBoundingClientRect().toString()) + scrollTop + 10;

    let tooltip;
    if (!document.getElementById("copy_tooltip"))
    {
        tooltip = document.createElement('div');
        tooltip.id = "copy_tooltip";
        tooltip.style.position = "absolute";
        tooltip.style.border = "1px solid black";
        tooltip.style.background = "#dbdb00";
        tooltip.style.opacity = "1";
        tooltip.style.transition = "opacity 0.3s";
        document.body.appendChild(tooltip);
    }
    else
    {
        tooltip = document.getElementById("copy_tooltip")
    }

    tooltip.style.opacity = "1";
    tooltip.style.left = x + "px";
    tooltip.style.top = y + "px";
    tooltip.innerHTML = message;
    setTimeout(function() { tooltip.style.opacity = "0"; }, 2000);
}

function select_all_and_copy(el)
{
    // Copy textarea, pre, div, etc.
    if (document.body.createTextRange)
    {
        // IE
        let textRange = document.body.createTextRange();
        textRange.moveToElementText(el);
        textRange.select();
        textRange.execCommand("Copy");
        tooltip(el, "Copied!");
    }
    else if (window.getSelection && document.createRange)
    {
        // non-IE
        let editable = el.contentEditable; // Record contentEditable status of element
        let readOnly = el.readOnly; // Record readOnly status of element
        el.contentEditable = true; // iOS will only select text on non-form elements if contentEditable = true;
        el.readOnly = false; // iOS will not select in a read only form element

        let range = document.createRange();
        range.selectNodeContents(el);
        let sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);

        // Does not work for Firefox if a textarea or input
        // Firefox will only select a form element with select()
        if (el.nodeName === "TEXTAREA" || el.nodeName === "INPUT")
        {
            el.select();
        }

        // iOS only selects "form" elements with SelectionRange
        if (el.setSelectionRange && navigator.userAgent.match(/ipad|ipod|iphone/i))
        {
            el.setSelectionRange(0, 999999);
        }

        // Restore previous contentEditable and readOnly states.
        el.contentEditable = editable;
        el.readOnly = readOnly;

        if (document.queryCommandSupported("copy"))
        {
            let successful = document.execCommand('copy');
            if (successful) tooltip(el, "Copied to clipboard.");
            else tooltip(el, "Press CTRL+C to copy");
        }
        else
        {
            if (!navigator.userAgent.match(/ipad|ipod|iphone|android|silk/i))
            {
                tooltip(el, "Press CTRL+C to copy");
            }
        }
    }
} // end function select_all_and_copy(el)

function make_copy_button(el)
{
    //let copy_btn = document.createElement('button');
    //copy_btn.type = "button";
    let copy_btn = document.createElement('span');
    copy_btn.style.border = "1px solid black";
    copy_btn.style.padding = "5px";
    copy_btn.style.cursor = "pointer";
    copy_btn.style.display = "inline-block";
    copy_btn.style.background = "lightgrey";

    el.parentNode.insertBefore(copy_btn, el.nextSibling);
    copy_btn.onclick = function() { select_all_and_copy(el); };

    //if (document.queryCommandSupported("copy") || parseInt(navigator.userAgent.match(/Chrom(e|ium)\/([0-9]+)\./)[2]) >= 42)
    // Above caused: TypeError: 'null' is not an object (evaluating 'navigator.userAgent.match(/Chrom(e|ium)\/([0-9]+)\./)[2]') in Safari
    if (document.queryCommandSupported("copy"))
    {
        // Desktop: Copy works with IE 4+, Chrome 42+, Firefox 41+, Opera 29+
        // Mobile: Copy works with Chrome for Android 42+, Firefox Mobile 41+
        //copy_btn.value = "Copy to Clipboard";
        copy_btn.innerHTML = "Copy to Clipboard";
    }
    else
    {
        // Select only for Safari and older Chrome, Firefox and Opera
        /* Mobile:
                Android Browser: Selects all and pops up "Copy" button
                iOS Safari: Selects all and pops up "Copy" button
                iOS Chrome: Form elements: Selects all and pops up "Copy" button
        */
        //copy_btn.value = "Select All";
        copy_btn.innerHTML = "Select All";

    }
}
/* Note: document.queryCommandSupported("copy") should return "true" on browsers that support copy
	but there was a bug in Chrome versions 42 to 47 that makes it return "false".  So in those
	versions of Chrome feature detection does not work!
	See https://code.google.com/p/chromium/issues/detail?id=476508
*/