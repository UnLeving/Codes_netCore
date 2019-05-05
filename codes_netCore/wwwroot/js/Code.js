var cntrlIsPressed = false;

$(document).keydown(function (event) {
    if (event.key === 'Control') {
        cntrlIsPressed = true;
    }
});

$(document).keyup(function () {
    cntrlIsPressed = false;
});

// forward region by middle btn click
$('body').on('mousedown', 'tbody td', function () {
    window.event.preventDefault();
    if (!cntrlIsPressed && event.button === 1) {
        document.getElementById("regionChange").value = $("#regionChange").val() + this.textContent.charAt(0);
        RegionChanged($("#regionChange").val());
    }
});

// back region by ctrl + middle btn click
$('body').on('mousedown', 'tbody td', function () {
    window.event.preventDefault();
    var R = $("#regionChange").val();
    if (cntrlIsPressed && R.length > 1 && event.button === 1) {
        cntrlIsPressed = false;
        R = R.slice(0, R.length - 1);
        document.getElementById("regionChange").value = R;
        RegionChanged(R);
    }
});

// DELETE ZONE

// select and delete codes in column 
$('body').on('contextmenu', 'thead th', function () {
    window.event.preventDefault();
    if (cntrlIsPressed === true) {
        cntrlIsPressed = false;
        return;
    }
    
    var codesIDs = [];
    var columnCells = [];
    var tableCodes = this.closest('table');
    var selectedColumn = parseInt(this.textContent, 10) + 2;
    for (var i = 1; i < tableCodes.rows.length; ++i) {
        if (tableCodes.rows[i].cells[selectedColumn].id !== "0") {
            codesIDs.push(tableCodes.rows[i].cells[selectedColumn].id);
            columnCells.push(tableCodes.rows[i].cells[selectedColumn]);
        }
    }
    if (codesIDs.length === 0) {
        document.getElementById("Logs").value = "Client: nothing to delete";
        return;
    }

    DeleteCodes(codesIDs, columnCells);
});

// delete single code  OR all codes by CTRL pressed
$('body').on('contextmenu', 'tbody td', function () {
    var codesIDs = [];
    var rowCells = [];
    var cells = [];
    if (cntrlIsPressed) {
        //rowCells = $(this.closest('tbody')).children();
        //for (var i = 0; i < rowCells.length; ++i) {
        //    for (var j = 2; j < 12; ++j) {
        //        if (rowCells[i].cells[j].id !== "0") {
        //            codesIDs.push(rowCells[i].cells[j].id);
        //            cells.push(rowCells[i].cells[j]);
        //        }
        //    }
        //}
        //if (codesIDs.length === 0) {
        //    document.getElementById("Logs").value = "Client: nothing to delete";
        //    cntrlIsPressed = false;
        //    window.event.preventDefault();
        //    return;
        //}
        DeleteAllTableCodes($("#regionChange").val());
        window.event.preventDefault();
        return;
    } else if (this.id < 0) {
        DeleteInheritedCode(this.id, this.textContent);
        window.event.preventDefault();
        return;
    } else {
        if (this.id === '0') {
            document.getElementById("Logs").value = "Client: nothing to delete";
            window.event.preventDefault();
            return;
        }
        codesIDs.push(this.id);
        cells.push(this);
    }

    DeleteCodes(codesIDs, cells);
    window.event.preventDefault();
});

// select and delete codes in row
$('body').on('contextmenu', 'tbody th', function () {
    window.event.preventDefault();
    if (cntrlIsPressed === true) {
        cntrlIsPressed = false;
        return;
    }
    var rowCells = $(this).parent().children('td');
    var codesIDs = [];
    var cells = [];

    for (var i = 0; i < rowCells.length; ++i) {
        if (rowCells[i].id !== "0") {
            codesIDs.push(rowCells[i].id);
            cells.push(rowCells[i]);
        }
    }

    if (codesIDs.length === 0) {
        document.getElementById("Logs").value = "Client: nothing to delete";
    } else if (codesIDs[0] < 0)
    {
        DeleteInheritedCode(rowCells[0].id, 0);
    }
    else
        DeleteCodes(codesIDs, cells);
});

function DeleteCodes(codesIDs, cells) {
    $('#loader').show();
    $.ajax({
        url: "/Codes/Delete",
        type: "POST",
        data: {
            ids: codesIDs
        },
        success: function () {
            $(cells).css('background-color', '#FFFFFF');
            document.getElementById("Logs").value = "200 OK";
            $('#loader').hide();
        },
        error: function (status) {
            document.getElementById("Logs").value = status.statusText;
            $('#loader').hide();
        }
    });
}

function DeleteInheritedCode(rootId, code) {
    $('#loader').show();
    $.ajax({
        url: "/Codes/DeleteInheritedCode",
        type: "POST",
        data: {
            Id: rootId,
            CountryId: $("#ddlCountries").val(),
            R: $("#regionChange").val(),
            Value: code
        },
        success: function () {
            document.getElementById("Logs").value = "200 OK";
            RegionChanged($("#regionChange").val());
            $('#loader').hide();
        },
        error: function (status) {
            document.getElementById("Logs").value = status.statusText;
            $('#loader').hide();
        }
    });
}

function DeleteAllTableCodes(R) {
    $('#loader').show();
    $.ajax({
        url: "/Codes/DeleteAllTableCodes",
        type: "POST",
        data: {
            R: R
        },
        success: function () {
            document.getElementById("Logs").value = "200 OK";
            RegionChanged($("#regionChange").val());
            $('#loader').hide();
        },
        error: function (status) {
            document.getElementById("Logs").value = status.statusText;
            $('#loader').hide();
        }
    });
}

// ADD ZONE

// select and add column 
$('body').on('click', 'thead th', function () {
    if (!IsNetworkSelected() || cntrlIsPressed === true) return;
    var codes = [];
    var cells = [];

    var table = this.closest('table');
    var selectedColumn = parseInt(this.textContent, 10) + 2;
    for (var i = 1; i < table.rows.length; ++i) {
        codes.push(table.rows[i].cells[selectedColumn].textContent);
        cells.push(table.rows[i].cells[selectedColumn]);
    }
    SendCodesOnServer(codes, cells);
});

// select and add cell or all cells by CTRL pressed
$('body').on('click', 'tbody td', function () {
    if (!IsNetworkSelected()) {
        return;
    }
    var codes = [];
    var cells = [];
    if (cntrlIsPressed === true) {
        cells = $(this.closest('tbody')).children();
        for (var i = 0; i < cells.length; ++i) {
            for (var j = 2; j < 12; ++j) {
                codes.push(cells[i].cells[j].textContent);
            }
        }
    }
    else {
        cells.push(this);
        codes.push(this.textContent);
    }

    SendCodesOnServer(codes, cells);
});

// select and add row
$('body').on('click', 'tbody th', function () {
    if (!IsNetworkSelected() || cntrlIsPressed === true) return;
    var codes = [];
    var cells = $(this).parent().children('td');
    for (var i = 0; i < cells.length; ++i) {
        codes.push(cells[i].textContent);
    }

    SendCodesOnServer(codes, cells);
});


function SendCodesOnServer(codes, cells) {
    var isTableSelected = cntrlIsPressed;
    $('#loader').show();
    $.ajax({
        url: "/Codes/AddCodes",
        type: "POST",
        data: {
            CountryId: $("#ddlCountries").val(),
            NetworkId: document.getElementById("NetworkIdSaver").value,
            R: $("#regionChange").val(),
            Values: codes
        },
        success: function () {
            if (isTableSelected) {
                AddedAllTableCodes_OnSuccess(cells);
            } else {
                $(cells).css('background-color', document.getElementById("HexSaver").value);
            }
            RegionChanged($("#regionChange").val());
            document.getElementById("Logs").value = "200 OK";
            $('#loader').hide();
        },
        error: function (status) {
            document.getElementById("Logs").value = status.statusText;
            $('#loader').hide();
        }
    });
}

function AddedAllTableCodes_OnSuccess(cells) {
    for (var i = 0; i < cells.length; ++i) {
        for (var j = 2; j < 12; ++j) {
            $(cells[i].cells[j]).css('background-color', document.getElementById("HexSaver").value);
        }
    }
}

function IsNetworkSelected() {
    if (document.getElementById("NetworkIdSaver").value === "0") {
        document.getElementById("Logs").value = "Client: network not selected";
        return false;
    } else {
        return true;
    }
}