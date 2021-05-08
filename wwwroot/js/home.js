function getLinks() {
    $.get("api/Files", function (fetchedLinks) {
        var newFileList = $('<ul id="fileList" />');

        $.each(fetchedLinks, function (index, value) {
            var li = $('<li/>').appendTo(fileList);
            $('<a/>').attr('href', value).text(value.split('/').pop()).appendTo(li);
            newFileList.append(li);
        })

        $('#fileList').replaceWith(newFileList);
    });
}

getLinks();

setInterval(function () {
    getLinks()
}, 5000);

Dropzone.options.fileUpload = {
    paramName: "file", // The name that will be used to transfer the file
    dictDefaultMessage: "Drop files here or click to upload",
    addRemoveLinks: true, // Allows for cancellation of file upload and remove thumbnail
    init: function () {
        myDropzone = this;
        myDropzone.on("success", function (file, response) {
            myDropzone.removeFile(file);
            getLinks();
        });
    }
};
