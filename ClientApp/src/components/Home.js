import React, { Component } from 'react';

import Dropzone from 'react-dropzone';
import axios from 'axios';
import classNames from 'classnames';


export class Home extends Component {
  static displayName = Home.name;

  onDrop = files => {
    console.log({ files })
    // Push all the axios request promise into a single array
    const uploaders = files.map(file => {
      // Initial FormData
      const formData = new FormData();
      formData.append("file", file);

      formData.append("name", "bobby");

      // Make an AJAX upload request using Axios
      return axios.post("/api/image", formData, {
        // using 
        headers: {
          "content-type": "multipart/form-data",
          "accept": "application/json"
        },
      }).then(response => {
        console.log({ response });
        this.setState({
          lastUploadedUrl: response.data.secure_url
        })
      })
    });

    // Once all the files are uploaded 
    axios.all(uploaders).then(() => {
      console.log("done");
    });
  }



  render() {
    return (
      <div>
        <Dropzone onDrop={this.onDrop}>
          {({ getRootProps, getInputProps, isDragActive }) => {
            return (
              <div
                {...getRootProps()}
                className={classNames('dropzone', { 'dropzone--isActive': isDragActive })}
              >
                <input {...getInputProps()} />
                {
                  isDragActive ?
                    <p>Drop files here...</p> :
                    <p>Here we go</p>
                }
              </div>
            )
          }}
        </Dropzone>
      </div >
    );
  }
}
