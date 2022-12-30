// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function load(items) {
    let map = new Map();
    var split = items.split(" - ");
    for (var a = 0; a < split.length; a++) {
        var item = split[a];
        var itemSplit = item.split(" adet ");
        var amount = parseInt(itemSplit[0]);
        var itemName = itemSplit[1];
        map.set(itemName, amount);
    }
    var amounts = $("input[id$='__Amount']");
    for (var i = 0; i < amounts.length; i++) {
        var amount = amounts[i].value;
        if (!amount || parseInt(amount) < 1) {
            var name = document.getElementById("name " + i).innerText;
            amount = 1;
            if (map.has(name)) {
                amount = map.get(name);
                document.getElementById("checkbox " + i).checked = true
            }
            document.getElementById(i + "__Amount").value = amount;
        }
    }
}

function calculate() {
    var text = document.getElementById("text");
    text.style.display = "block";
    var amounts = $("input[id$='__Amount']");
    var prices = $("td[id='price']");
    var tot = 0.0;
    for (var i = 0; i < amounts.length; i++) {
        var amount = amounts[i].value;
        if (!amount || parseInt(amount) < 1) {
            amount = 1;
            document.getElementById(i + "__Amount").value = amount;
        }
        var price = parseFloat(prices[i].innerHTML.replace('₺', '').replace(',', '.'));
        if (document.getElementById("checkbox " + i).checked && parseInt(amount))
            tot += parseInt(amount) * price;
    }
    text.innerHTML = "Toplam ₺" + tot;
}

function calculateLoad() {
    var elements = $("li[id^='entry']");
    var total = 0.0;
    for (var i = 0; i < elements.length; i++) {
        var element = elements[i];
        var price = parseFloat(element.getAttribute("price").replace(',', '.'));
        var amount = parseInt(element.getAttribute("amount"));
        var result = price * amount;
        element.innerHTML += " = ₺" + result;
        total += result;
    }
    document.getElementById("text").innerHTML = "Toplam ₺" + total;
}

function selectAll() {
    var checkbox = document.getElementById("allSelect");
    var checked = checkbox.checked;
    var checkboxes = $("input[id^='checkbox']");
    for (var i = 0; i < checkboxes.length; i++) {
        var element = checkboxes[i];
        element.checked = checked;
    }
    calculateReports();
}

function selectCheckbox(checked) {
    var allSelect = document.getElementById("allSelect");
    if (!checked) {
        allSelect.checked = false;
    } else {
        var checkboxes = $("input[id^='checkbox']");
        var containsUnchecked = false;
        for (var i = 0; i < checkboxes.length; i++) {
            var element = checkboxes[i];
            if (!element.checked) {
                containsUnchecked = true;
                break;
            }
        }
        if (!containsUnchecked) allSelect.checked = true;
    }
    calculateReports();
}

function calculateReports() {
    var checkboxes = $("input[id^='checkbox']");
    var total = 0.0;
    var found = false;
    for (var i = 0; i < checkboxes.length; i++) {
        var element = checkboxes[i];
        if (element.checked) {
            found = true;
            var price = document.getElementById("price " + element.value);
            var priceFloat = parseFloat(price.innerHTML.replace('₺', '').replace(',', '.'));
            total += priceFloat;
        }
    }
    var selected = document.getElementById("selected");
    if (found) {
        selected.style.display = "";
        selected.innerHTML = "Seçili Raporların Toplamı ₺" + total;
    } else selected.style.display = "none";
}

function showPopUp() {
    window.open('/Report/ZReport', "PopupWindow", 'width=800px,height=800px,top=150,left=250');
}