import React, { useContext, Fragment } from 'react';
import { Container, Segment, Header, Button, Image, Divider } from 'semantic-ui-react';
import { Link } from 'react-router-dom';
import { RootStoreContext } from '../../app/stores/rootStore';
import LoginForm from '../user/LoginForm';
import RegisterForm from '../user/RegisterForm';

const HomePage = () => {
  const token = window.localStorage.getItem('jwt');
  const rootStore = useContext(RootStoreContext);
  const { user, isLoggedIn } = rootStore.userStore;
  const {openModal} = rootStore.modalStore;
  return (
    <Segment inverted textAlign='center' vertical className='masthead'>
      <Container text>
        <Header as='h1' inverted>
          <Image
            size='massive'
            src='/assets/logo.png'
            alt='logo'
            style={{ marginBottom: 12 }}
          />
        </Header>
        <Header as='h2' inverted content={`Welcome to Reactivitities by fotis`} />
      
        {isLoggedIn && user && token ? (
          <Fragment>
            <Header as='h2' inverted content={`Welcome back ${user.displayName}`} />
            <Button as={Link} to='/activities' size='huge' inverted>
              Go to activities!
            </Button>
          </Fragment>
        ) : (
          <Fragment>
            
       
       
          <Button onClick={() => openModal(<LoginForm />)} size='huge' inverted>
            Login
          </Button>
          <Button onClick={() => openModal(<RegisterForm />)} size='huge' inverted>
            Register
          </Button>
        </Fragment>
        )}
        <Divider horizontal></Divider>
          <Fragment>
          <body>Inspired by <a href="https://www.udemy.com/share/101Ae6BkoSdFZUQHg=/">Niel Cumming course</a></body>
          
        </Fragment>
      </Container>
    </Segment>
  );
};

export default HomePage;
