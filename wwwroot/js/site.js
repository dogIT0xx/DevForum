// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function encodeImageFile() {
    let files = document.getElementById("inputImageFile").files
    let fileReader = new FileReader()
    fileReader.onload = function () {
        console.log('RESULT', fileReader.result)
        // fileReader.result - base64
    }

    for (let i = 0; i < files.length; i++) {
        fileReader.readAsDataURL(files[i])
    }
}