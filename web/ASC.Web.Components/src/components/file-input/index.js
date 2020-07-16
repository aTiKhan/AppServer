import React, { Component } from "react";
import PropTypes from "prop-types";
import styled, { css } from 'styled-components';

import IconButton from '../icon-button';
import TextInput from '../text-input';


const StyledFileInput = styled.div`
  display: flex;
  position: relative;

  width: ${props =>
        (props.scale && '100%') ||
        (props.size === 'base' && '173px') ||
        (props.size === 'middle' && '300px') ||
        (props.size === 'big' && '350px') ||
        (props.size === 'huge' && '500px') ||
        (props.size === 'large' && '550px')
    };

  .icon {
    display: flex;
    align-items: center;
    justify-content: center;

    position: absolute;
    right: 0;

    width: ${props => props.size === 'large' ? '48px'
      : props.size === 'huge' ? '38px'
        : props.size === 'big' ? '37px'
          : props.size === 'middle' ? '36px'
            : '30px'
    };

    height: ${props => props.size === 'large' ? '43px'
      : props.size === 'huge' ? '37px'
        : props.size === 'big' ? '36px'
          : props.size === 'middle' ? '36px'
            : '30px'
    };

    margin: 0;
    border: 1px solid;
    border-radius: 0 3px 3px 0;

    border-color: ${props => (props.hasError && '#c30') || (props.hasWarning && '#f1ca92') || (props.isDisabled && '#ECEEF1')|| '#D0D5DA'};
    
    :hover{
        border-color: ${props => (props.hasError && '#c30') || (props.hasWarning && '#f1ca92') || (props.isDisabled && '#ECEEF1')|| '#A3A9AE'};
    }

    :focus{
        border-color: ${props => (props.hasError && '#c30') || (props.hasWarning && '#f1ca92') || (props.isDisabled && '#ECEEF1')|| '#2DA7DB'};
    }
    
    cursor: ${props => (props.isDisabled ? 'default' : 'pointer')}
  }
`;

class FileInput extends Component { 
  constructor(props) {
    super(props);

    this.inputRef = React.createRef();

    this.state = {
      fileName: '',
      path: ''
    }

  }

  onIconFileClick = e => {
    console.log('click')
    e.target.blur();
    this.inputRef.current.click();
  }

  onChangeFile = e => this.setState({ 
    path: e.target.value 
  });

  onInputFile = () => this.setState({
    fileName: this.inputRef.current.files[0].name
  });

  render() {
    const { fileName } = this.state;
    const { 
      size, 
      isDisabled, 
      scale, 
      hasError,
      hasWarning,
      ...rest 
    } = this.props;

    let iconSize = 0;

    switch (size) {
      case 'base':
        iconSize = 15;
        break;
      case 'middle':
        iconSize = 15;
        break;
      case 'big':
        iconSize = 16;
        break;
      case 'huge':
        iconSize = 16;
        break;
      case 'large': 
        iconSize = 16;
        break;
    }
    
    return( 
      <StyledFileInput 
        size={size} 
        scale={scale}
        hasError={hasError}
        hasWarning={hasWarning}
        isDisabled={isDisabled}
      >

        <TextInput
          value={fileName}
          onChange={this.onChangeFile}
          onFocus={this.onIconFileClick}
          size={size}
          isDisabled={isDisabled}
          hasError={hasError}
          hasWarning={hasWarning}
        />
          <input
            type="file"
            onInput={this.onInputFile}
            ref={this.inputRef}
            style={{ display: 'none' }}
          />
        <div className="icon">
          <IconButton 
            iconName={"CatalogFolderIcon"}
            size={iconSize}
            onClick={this.onIconFileClick}
            isDisabled={isDisabled}
          />
        </div>
      </StyledFileInput>
    )
  }
}

FileInput.propTypes = {
  style: PropTypes.oneOfType([PropTypes.object, PropTypes.array]),
  placeholder: PropTypes.string,
  size: PropTypes.string
}

export default FileInput;