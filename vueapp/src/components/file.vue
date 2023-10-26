<template>
    <div id="app">
        <!-- add setting -->
        <file-pond name="test" ref="pond" label-idle="Drop files here..." v-bind:allow-multiple="true"
            accepted-file-types="image/jpeg, image/png" v-bind:files="myFiles" v-on:init="handleFilePondInit" />
    </div>
</template>
  
<script>
// Import Vue FilePond
import vueFilePond, { setOptions } from 'vue-filepond';


setOptions({
    labelIdle: 'hello!',
    server: {
        process: (fieldName, file, metadata, load, error, progress, abort) => {
            const formData = new FormData();
            formData.append("file", file, file.name);
            //randomly generate a case id
            formData.append("CaseID", Math.floor(Math.random() * 1000000000) );
            console.log(123);
            const request = new XMLHttpRequest();
            request.open('POST', "https://localhost:7067/api/file");
            // Setting computable to false switches the loading indicator to infinite mode
            request.upload.onprogress = (e) => {
                progress(e.lengthComputable, e.loaded, e.total);
            };

            request.onload = function () {
                if (request.status >= 200 && request.status < 300) {
                    load(request.responseText);// the load method accepts either a string (id) or an object
                }
                else {
                    error('Error during Upload!');
                }
            };

            request.send(formData);
            //expose an abort method so the request can be cancelled
            return {
                abort: () => {
                    // This function is entered if the user has tapped the cancel button
                    request.abort();
                    // Let FilePond know the request has been cancelled
                    abort();
                }
            };
        }, // we've not implemented these endpoints yet, so leave them null!
        fetch: null,
        remove: (source, load, error) => {
            // Send request to delete file
            const request = new XMLHttpRequest();
            request.open('DELETE', source);
            request.onload = function () {
                if (request.status >= 200 && request.status < 300) {
                    load(request.responseText);
                }
                else {
                    error('Error during delete');
                }
            };
            request.send();
        }
    }
})

// Import FilePond styles
import "filepond/dist/filepond.min.css";

// Import FilePond plugins
// Please note that you need to install these plugins separately

// Import image preview plugin styles
import "filepond-plugin-image-preview/dist/filepond-plugin-image-preview.min.css";

// Import image preview and file type validation plugins
import FilePondPluginFileValidateType from "filepond-plugin-file-validate-type";
import FilePondPluginImagePreview from "filepond-plugin-image-preview";

// Create component
const FilePond = vueFilePond(
    FilePondPluginFileValidateType,
    FilePondPluginImagePreview
);




export default {
    name: "app",
    data: function () {
        return { myFiles: [] };
    },
    methods: {
        handleFilePondInit: function () {
            console.log("FilePond has initialized");

            // FilePond instance methods are available on `this.$refs.pond`
        }
    },
    components: {
        FilePond,
    },
    Created() {
        this.ref.pond.set
    },
};
</script>